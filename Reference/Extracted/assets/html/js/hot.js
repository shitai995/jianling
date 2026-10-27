/**
 * 
 */

var url_index_g = p_util.getUrl("ad/j_index.g", 60*8);
var url_rank_g = p_util.getUrl("ad/j_rank.g", 60*8);
var url_search_g = p_util.getUrl("ad/j_search.g", 60*24);

var tmplNewGame = $.tmpl('tmpl-new-game');
var tmplHotGame = $.tmpl('tmpl-hot-game');
console.log(tmplNewGame);

var $input = $('input');
var $form = $('form');
var $keyword = $('#keyword');
var $submit = $('submit');
var hasSearchBoxTips = true;

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


function loadJson(jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}

	window.g_resBaseUrl = jsonObj.resourceUrl;
	
	// 新品推荐
	$newGameCategoryUl = $.q("#w-new-game-category ul");
	$.empty($newGameCategoryUl);
	for (i = 0; jsonObj.value.adGameList && jsonObj.value.adGameList[i]; i++) {
		var _html = tmplNewGame(jsonObj.value.adGameList[i]);
		console.log(_html);
		$newGameCategoryUl.appendChild($.docFrag(_html));
	}
	
	// 热门游戏（评分)
	$hotGameCategoryUl = $.q("#w-hot-game-category ul");
	$.empty($hotGameCategoryUl);
	$.each(jsonObj.value.hotGameList, function(key, val) {
		var _html = tmplHotGame(val);
		console.log(_html);
		$hotGameCategoryUl.appendChild($.docFrag(_html));
	});
	
	// 热词
	$.each(jsonObj.value.keywordList, function(key, val) {
		$keyword.val(val.keyword);
		hasSearchBoxTips = false;
	});
	
	g_pageTool.updateBtn(1, jsonObj.value.hasMore);
	loadComplete();
}

$(document).ready(function() {
	
	console.log(url_index_g);
	
	if(jsonData == '') {
		onGetPage(1, $('body'));
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
	/**
	 * 获取分页
	 */
	function onGetPage(newPageNo, btn) {
		loading(btn);
		
		/*
		// 确定是搜索关键词，还是使用榜单
		console.log('keyword_pre:'+$keyword.val());
		var keyword = hasSearchBoxTips ? '' : $keyword.val();
		console.log('keyword:'+keyword);
		*/
		if(newPageNo == 1/* && keyword==''*/) {
			$.getJSON(url_index_g, {rankid: HOT_RANK_ID}, loadJson);
			return;
		}

		//var url = keyword!='' ? url_search_g : url_rank_g;
		//var params = keyword!='' ? {pageno: newPageNo, keyword: keyword} : {pageno: newPageNo, rankid: HOT_RANK_ID};
		var url = url_rank_g;
		var params = {pageno: newPageNo, rankid: HOT_RANK_ID};
		
		$.getJSON(url, params, function(jsonObj, stat) {
				if (jsonObj && jsonObj.value) {
					$.empty($hotGameCategoryUl);
					$.each(jsonObj.value, function(index, val){
						var _html = tmplHotGame(val);
						console.log(_html);
						$hotGameCategoryUl.appendChild($.docFrag(_html));
					});
				}
				
				//数据加载成功
				g_pageTool.updateBtn(newPageNo, jsonObj.more==1);
				loadComplete();
			}
		);

		
		
	} // $$$ function onGetPage(eventObj) {
	
	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	// 输入框点击时取消提示语
	$input.focus(function(evtO) {
		if (hasSearchBoxTips) { $(this).val(''); hasSearchBoxTips = false; }
	});

	// 提交按钮
	$form.submit(function(evtO) {
		if (hasSearchBoxTips) return false; //
		window.location='game-search.html?num=10&sid='+sid+'&keyword='+$keyword.val();
		//onGetPage(1, $('body'));
		
		return false;
	});
}); // $$$ $(document).ready