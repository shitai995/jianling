var isDetailStatus = true;

var resId = p_util.getUrlQueryPara('resId');
console.log(resId);

var url_gameDetail_g = p_util.getUrl("ad/j_gameDetail.g", g_refresh_interval);
var url_reportDown_g = p_util.getUrl("ad/j_down.g");
var url_ad_g = p_util.getUrl("ad/j_adList.g", 60*24);
var url_feed_g = p_util.getUrl("ad/j_feed.g", g_refresh_interval);

var pkgName = "";
var picListCache = ["","",""];
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

/**
 * 渲染页面展示
 */
function loadJson(jsonObj, stat) {

	if(p_util.isPreLoad()) {
		return;
	}
	
	console.log('before:' + url_gameDetail_g);
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	
	pkgName = jsonObj.value.gameInfo.pkgName;
	resId = jsonObj.value.gameInfo.resId;
	picListCache = jsonObj.value.gameInfo.picList;
	
	html = tmpl('tmpl-game-title', jsonObj.value);
	console.log(html);
	$(".game-title").html(html);
	
	html = tmpl('tmpl-game-detail', jsonObj.value.gameInfo);
	console.log(html);
	$(".game-detail").html(html);
	
	html = tmpl('tmpl-game-operation-1', jsonObj.value);
	console.log(html);
	$("#game-operation-1 ul").html(html);
	
	html = tmpl('tmpl-game-operation-2', jsonObj.value);
	console.log(html);
	$("#game-operation-2 ul").html(html);
	
	loadFeed(jsonObj.value.feedList, jsonObj.value.hasMore);
	
	if(jsonObj.value.hasPlayed || p_pkg.isCurGame(cpId, gameId, jsonObj.value.gameInfo.pkgName) || p_pkg.checkApk(jsonObj.value.gameInfo.pkgName)){
		//p_pkg.showMsg(cpId + '|' + gameId + '|' + jsonObj.value.gameInfo.pkgName + '|' + p_pkg.isCurGame(cpId, gameId, jsonObj.value.gameInfo.pkgName));
		
		$('#w-game-detail-ul').css('display', 'none');
		$('.game-detail a').rmCls('open');
	}
	
	
	//处理上一张图片事件
	$('.preview-img-pre').click(function(evtO) {
		$this = $(this);
		console.log("hasCls(on):"+$this.hasCls('on'));
		if (!$this.hasCls('on')) { 
			return false; 
		}//不可点
		
		$('.preview-img-operation ul li a.on').rmCls('on').parent().prev().children().addCls('on'); //圆点移到上一个
		
		$('.game-preview>ul .on').rmCls('on').prev().addCls('on');//图片移到上一个
		
		var $img = $('.game-preview>ul .on img'); //加载图片
		$img.attr("src", picListCache[$img.attr("alt")]);
		
		$('.preview-img-next').addCls('on'); //下一个按钮，可点
		if ($('.game-preview>ul .on').prev().size() == 0) { $this.rmCls('on'); } // 是否还存在下一个
		
	});
	
	//处理下一张图片事件
	$('.preview-img-next').click(function(evtO) {
		$this = $(this);
		console.log("hasCls(on):"+$this.hasCls('on'));
		if (!$this.hasCls('on')) { 
			return false; 
		}//不可点
		
		$('.preview-img-operation ul li a.on').rmCls('on').parent().next().children().addCls('on'); //圆点移到下一个
		
		$('.game-preview>ul .on').rmCls('on').next().addCls('on');//图片移到下一个
		
		var $img = $('.game-preview>ul .on img'); //加载图片
		$img.attr("src", picListCache[$img.attr("alt")]);
		
		$('.preview-img-pre').addCls('on'); //上一个按钮，可点
		if ($('.game-preview>ul .on').next().size() == 0) { $this.rmCls('on'); } // 是否还存在下一个
		
		
	});
	
	
	$('.game-detail>h1').click(function(evtO) {
		$ul = $('#w-game-detail-ul');
		if ($ul.css('display') == 'block') {
			$ul.css('display', 'none');
			$('.game-detail a').rmCls('open');
		}
		else {
			$ul.css('display', 'block');
			$('.game-detail a').addCls('open');
		}
	});
	
	g_pageTool.updateBtn(1, jsonObj.value.hasMore);
	loadComplete();
	
	console.log('end:' + url_gameDetail_g);
	
} // loadJSON

// ready
$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_gameDetail_g, {cpId: cpId, gameId: gameId}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	// 是否加载活动消息
	if( p_pkg.checkApk(pkgName) ){
		$.getJSON(url_ad_g, {pageId: 2112}, function(jsonObj, stat) {
			if(jsonObj.extra.adContent != "") {
				$(".tips-confirm").html(jsonObj.extra.adContent);
				$(".pay").css('display', 'block');
			}else {
				$(".pay").css('display', 'none');
			}
		});
	}

}); // $$$ $(document).ready
