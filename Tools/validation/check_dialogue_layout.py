#!/usr/bin/env python3
"""Run the recovered Java StyledText with deterministic metrics and compare the Unity formatter.

prepare writes a corpus/reference; compare consumes the production output exported through Unity MCP.
The Font stub isolates pagination semantics; it does not model Android or Unity glyph metrics.
"""
import argparse
import base64
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Reference/Validation'
SOURCE = ROOT / 'Reference/Decompiled/sources/com/dobetter'


def prepare(jdk):
    texts = []
    seen = set()
    pattern = re.compile(r'^x27\(\s*("(?:\\.|[^"\\])*")\s*,\s*("(?:\\.|[^"\\])*")')
    for path in sorted((ROOT / 'UnityProject/Assets/Resources/Original/Scripts').rglob('*.json')):
        for instruction in json.loads(path.read_text()).get('instructions', []):
            match = pattern.match(instruction)
            if not match:
                continue
            text = json.loads(match[2])
            if text not in seen:
                seen.add(text)
                texts.append({'text': text, 'width': 236})
    source_count = len(texts)
    samples = ['', '甲', '甲乙', '甲乙丙', '甲\n', '\n', '\n\n', '甲\n\n乙\n',
               'ABC甲def乙12!丙', '<c=ff0000>甲乙<c=,00ff00,2>丙丁</c>戊己</c>庚',
               '甲<c=00ff00>乙</c>丙', '<c=ff0000>甲乙</c>', '<c=ff0000>',
               '甲<i=2>乙丙', '甲<c=,ff0000,0>乙丙丁\n戊己庚</c>辛']
    for text in samples:
        for width in [18, 36, 55, 236]:
            texts.append({'text': text, 'width': width})
    (OUT / 'dialogue-layout-corpus.json').write_text(json.dumps(texts, ensure_ascii=False))
    stubs = {
        'common/Font.java': '''package com.dobetter.common;
public class Font { public static int fontChinaW=18,fontChinaH=24;
public int stringWidth(String s){int n=0;for(char c:s.toCharArray())n+=c>127?183:(c>='A'&&c<='Z'?111:94);return n/10;}}
''',
        'common/Tool.java': '''package com.dobetter.common;
public class Tool { public static int countTimes=0;
public static String[] getStringArray(String s,char c){return s.split(",",-1);}
public static void drawString(Graphics g,String s,int x,int y,int f,int b,byte t){} }
''',
        'common/Graphics.java': 'package com.dobetter.common;public class Graphics{public void setClip(int x,int y,int w,int h){}public void restore(){}}',
        'common/Common.java': 'package com.dobetter.common;public class Common{public static boolean isOKPressed(boolean b){return false;}public static void setUIClip(Graphics g){}}',
        'action/ActionSet.java': 'package com.dobetter.action;import com.dobetter.common.Graphics;public class ActionSet{public int actionNum=0;public void drawFrameCycle(Graphics g,int a,int b,int x,int y,boolean f){}public void releasImg(){}}',
    }
    harness = '''import java.util.*;import java.io.*;import java.nio.charset.StandardCharsets;import com.dobetter.common.*;
public class DialogueReference {
public static void main(String[] args)throws Exception{
BufferedReader r=new BufferedReader(new InputStreamReader(System.in,StandardCharsets.UTF_8));String row;
while((row=r.readLine())!=null){String[] p=row.split("\\t",-1);String text=new String(Base64.getDecoder().decode(p[1]),StandardCharsets.UTF_8);
StyledText st=new StyledText(text,Integer.parseInt(p[0]),2,new Font(),24,null,16);
StringJoiner lines=new StringJoiner("|");
for(Object raw:st.content){Vector line=(Vector)raw;Style style=new Style();StringJoiner spans=new StringJoiner(";");
for(Object ob:line){if(ob instanceof Style)style=(Style)ob;else if(ob instanceof String){String s=(String)ob;
spans.add(Base64.getEncoder().encodeToString(s.getBytes(StandardCharsets.UTF_8))+":"+new Font().stringWidth(s)+":"+style.fontColor+":"+style.backColor+":"+style.fontType);}}
lines.add(spans.toString());}
System.out.println(st.content.size()+"\\t"+lines.toString());}}}
'''
    with tempfile.TemporaryDirectory(prefix='gujian-dialogue-java-') as tmp:
        directory = Path(tmp)
        for name in ['StyledText', 'Style']:
            dst = directory / 'com/dobetter/common' / (name + '.java')
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(SOURCE / 'common' / (name + '.java'), dst)
        for name, source in stubs.items():
            dst = directory / 'com/dobetter' / name
            dst.parent.mkdir(parents=True, exist_ok=True)
            dst.write_text(source)
        (directory / 'DialogueReference.java').write_text(harness)
        subprocess.run([str(jdk / 'bin/javac'), '-encoding', 'UTF-8', *map(str, directory.rglob('*.java'))], check=True, capture_output=True)
        input_text = '\n'.join(str(case['width']) + '\t' + base64.b64encode(case['text'].encode()).decode() for case in texts) + '\n'
        result = subprocess.run([str(jdk / 'bin/java'), '-cp', tmp, 'DialogueReference'], input=input_text, text=True, check=True, capture_output=True)
    rows = result.stdout.splitlines()
    assert len(rows) == len(texts), (len(rows), len(texts), result.stderr)
    reference = []
    for row in rows:
        count, data = row.split('\t', 1)
        lines = []
        if int(count):
            for line in data.split('|'):
                spans = []
                for value in line.split(';') if line else []:
                    text, width, foreground, edge, font_type = value.split(':')
                    spans.append(dict(text=base64.b64decode(text).decode(), width=int(width), foreground=int(foreground), edge=int(edge), fontType=int(font_type)))
                lines.append(spans)
        assert len(lines) == int(count)
        reference.append(lines)
    (OUT / 'dialogue-layout-reference.json').write_text(json.dumps(dict(source_texts=source_count, cases=len(texts), lines=reference), ensure_ascii=False))
    print(json.dumps(dict(source_texts=source_count, cases=len(texts), reference='original StyledText.java / Style.java, stub Font'), ensure_ascii=False))


def compare():
    reference = json.loads((OUT / 'dialogue-layout-reference.json').read_text())
    actual = json.loads((OUT / 'dialogue-layout-production.json').read_text())
    corpus = json.loads((OUT / 'dialogue-layout-corpus.json').read_text())
    assert len(actual) == len(reference['lines'])
    differences = [dict(index=i, text=corpus[i], actual=a, expected=b) for i, (a, b) in enumerate(zip(actual, reference['lines'])) if a != b]
    report = dict(passed=not differences, source_texts=reference['source_texts'], cases=len(actual), differences=differences,
                  scope='Production C# versus recovered Java StyledText / Style with identical deterministic integer width metrics; Android glyphs are not compared.',
                  sha256={str(p.relative_to(ROOT)): hashlib.sha256(p.read_bytes()).hexdigest() for p in [OUT / ('dialogue-layout-' + suffix + '.json') for suffix in ['corpus', 'reference', 'production']]})
    (OUT / 'dialogue-layout-parity.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({k: v for k, v in report.items() if k != 'sha256'}, ensure_ascii=False)[:5000])
    if differences:
        raise SystemExit(1)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['prepare', 'compare'])
    parser.add_argument('--jdk', type=Path, default=Path('/Applications/Unity/Hub/Editor/6000.5.5f1/PlaybackEngines/AndroidPlayer/OpenJDK'))
    args = parser.parse_args()
    prepare(args.jdk) if args.action == 'prepare' else compare()
