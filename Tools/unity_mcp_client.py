"""Run explicit JSON requests against the project's local Unity MCP server.

Requires the MCP Python SDK (available in the local mcpforunityserver runtime).
Example: python Tools/unity_mcp_client.py Workbench/staging/mcp-requests.json
Request entries: {"resource": "mcpforunity://editor/state"},
{"tool": "manage_editor", "arguments": {"action": "play"}},
or {"schema": "execute_code"}. Calls run sequentially; mutations are not retried.
"""

import argparse
import asyncio
import json
import sys
from pathlib import Path
from urllib.parse import urlparse

from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client


def compact_result(result):
    data = result.model_dump(mode="json", exclude_none=True)
    # Do not print binary payloads into terminal transcripts.
    for block in data.get("content", []) + data.get("contents", []):
        if block.get("type") in ("image", "audio") and "data" in block:
            block["data"] = "<binary payload omitted>"
        if "blob" in block:
            block["blob"] = "<binary payload omitted>"
    return data


def tool_failed(response):
    status = response.structuredContent or {}
    if isinstance(status.get("result"), dict):
        status = status["result"]
    return bool(response.isError) or status.get("success") is False


async def run(args):
    requests = json.loads(args.requests.read_text(encoding="utf-8-sig"))
    if not isinstance(requests, list):
        raise ValueError("Request file must contain a JSON array")
    for request in requests:
        if not isinstance(request, dict) or sum(
            key in request for key in ("resource", "tool", "schema")
        ) != 1:
            raise ValueError("Each request needs exactly one of resource, tool, schema")
    endpoint = urlparse(args.url)
    if endpoint.scheme != "http" or endpoint.hostname not in ("127.0.0.1", "localhost", "::1"):
        raise ValueError("Only a local HTTP MCP endpoint is supported")
    failed = False
    async with streamablehttp_client(args.url) as (read, write, _):
        async with ClientSession(read, write) as session:
            await session.initialize()
            if args.instance:
                selected = await session.call_tool("set_active_instance", {"instance": args.instance})
                if tool_failed(selected):
                    raise RuntimeError(str(selected))
            schemas = None
            for request in requests:
                if "schema" in request:
                    if schemas is None:
                        schemas = {tool.name: tool for tool in (await session.list_tools()).tools}
                    result = schemas[request["schema"]].model_dump(mode="json", exclude_none=True)
                elif "resource" in request:
                    result = compact_result(await session.read_resource(request["resource"]))
                else:
                    response = await session.call_tool(request["tool"], request.get("arguments", {}))
                    result = compact_result(response)
                    failed = failed or tool_failed(response)
                print(json.dumps({"request": request, "result": result}, ensure_ascii=False), flush=True)
                if failed:
                    break
    return 1 if failed else 0


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("requests", type=Path)
    parser.add_argument("--url", default="http://127.0.0.1:8080/mcp")
    parser.add_argument("--instance")
    sys.exit(asyncio.run(run(parser.parse_args())))
