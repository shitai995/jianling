var isDetailStatus = true;

var resId = p_util.getUrlQueryPara('resId');
console.log(resId);

var url_gameDetail_g = p_util.getUrl("ad/j_gameDetail.g", g_refresh_interval);
var url_reportDown_g = p_util.getUrl("ad/j_down.g");
var url_ad_g = p_util.getUrl("ad/j_adList.g", 60*24);
var url_feed_g = p_util.getUrl("ad/j_feed.g", g_refresh_interval);


var tmplFeed = tmpl('tmpl-feed');
var $feed = $(".feed-list");
var $ul = $(".feed-list ul");
var $warn = $(".nodata-wrapper");

//初始化分页工具
var g_pageTool = new LoadingPageBtn($('#btn-1st'), $('#btn-prev'), $('#btn-next'));

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

function loading(bg) {
	bg.append($('<div style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));
}

function loadComplete() {
	if(g_lc.canvasElem.parentNode) {
		g_lc.canvasElem.parentNode.removeChild(g_lc.canvasElem);
	}
}

loading($('body'));


/**
 * 渲染feed列表
 */
function loadFeed(jsonObj, hasMore) {
	if(p_util.isPreLoad()) {
		return;
	}
	
	var dataCnt = 0;
	if (jsonObj) {
		$ul.html('');
		$.each(jsonObj, function(index, val){
			if(val.content!='') {
				var _html = tmplFeed(val);
				console.log(_html);
				$ul.append(_html);
				dataCnt++;
			}

		});
	}
	
	// 隐藏空数据提示框
	if(dataCnt == 0 && g_pageTool.pageNo==1) {
		$warn.css('display', 'none');
		$feed.css('display', 'none');
	}
	else {
		$warn.css('display', 'none');
	};
}

/**
 * 获取更多feed
 */
function onGetPage(newPageNo, btn) {
	loading(btn);
	
	if(newPageNo == 1) {
		$.getJSON(url_gameDetail_g, {cpId: cpId, gameId: gameId}, loadJson);
		return;
	}
	
	$.getJSON(url_feed_g, { pageno: newPageNo}, function(jsonObj, stat) {
		// 加载数据
		loadFeed(jsonObj.value, jsonObj.more);
		
		// 数据加载成功
		g_pageTool.updateBtn(newPageNo, jsonObj.more);
		loadComplete();
	});
}

function loadJson(jsonObj, stat) {
	
	if(p_util.isPreLoad()) {
		return;
	}
	
	console.log('before:' + url_gameDetail_g);
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	resId = jsonObj.value.gameInfo.resId;

	if (jsonObj && jsonObj.value) {
		html = tmpl('tmpl-game-title', jsonObj.value);
		console.log(html);
		$(".game-title").html(html);
		
		html = tmpl('tmpl-game-operation-1', jsonObj.value);
		console.log(html);
		$("#game-operation-1 ul").html(html);
		
		html = tmpl('tmpl-game-operation-2', jsonObj.value);
		console.log(html);
		$("#game-operation-2 ul").html(html);

	}

	loadFeed(jsonObj.value.feedList, jsonObj.value.hasMore);
	
	g_pageTool.updateBtn(1, jsonObj.value.hasMore);
	loadComplete();
	
	console.log('end:' + url_gameDetail_g);
	
}
// ready
$(document).ready(function() {
	
	$.getJSON(url_gameDetail_g, {cpId: cpId, gameId: gameId}, loadJson);
	
	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	$.getJSON(url_ad_g, {pageId: 2112}, function(jsonObj, stat) {
		if(p_util.isPreLoad()) {
			return;
		}
		
		if(jsonObj.extra.adContent != "") {
			$(".tips-confirm").html(jsonObj.extra.adContent);
			$(".pay").css('display', 'block');
		}else {
			$(".pay").css('display', 'none');
		}
	});
	
	
}); // $$$ $(document).ready

