/**
 * 
 */

var url_search_g = p_util.getUrl("ad/j_search.g", 60*24);
var url_hotwords_g = p_util.getUrl("ad/j_hotwords.g");

var tmplHotGame = $.tmpl('tmpl-hot-game');
var tmplHotwords = $.tmpl('tmpl-hotwords-list');
console.log(tmplHotGame);

var $searchTitle = $('#search-title');
var $btnPrev = $('#btn-prev');
var $btnNext = $('#btn-next');
var $btn1st = $('#btn-1st');
var $input = $('input');
var $form = $('form');
var $keyword = $('#keyword');
var $submit = $('submit');
var $hotwords = $('#hotwords');
var $hotGameCategoryUl = $.q("#w-hot-game-category ul");
var hasSearchBoxTips = true;
var keywordClsList = ['first', 'second', 'third', 'fourth', 'fifth', 'sixth'];
var keywordCls = '';
var keyword = decodeURI(p_util.getUrlQueryPara('keyword', ''));

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
	
	if (jsonObj && jsonObj.value) {
		$hotwords.empty();
		var id = 0;
		$.each(jsonObj.value, function(index, val){
			keywordCls = keywordClsList[id++];
			var _html = tmplHotwords(val);
			console.log(_html);
			$hotwords.append(_html);
		});

		// 热词
		if(keyword == '' && id > 0) {
			var kId = Math.floor(id*Math.random());
			$keyword.val(jsonObj.value[kId].keyword);
			hasSearchBoxTips = false;
		}


	} // $$$ if
	
	loadComplete();
}


/**
 * 获取分页
 */
function onGetPage(newPageNo, btn) {

	// 搜索关键词
	var keyword = hasSearchBoxTips ? '' : $keyword.val();
	
	if(keyword=='') {
		p_pkg.showMsg('请输入搜索关键词')
		return;
	}
	
	$searchTitle.show();
	
	loading(btn);
	$.getJSON(url_search_g, {pageno: newPageNo, keyword: keyword}, function(jsonObj, stat) {
			
			pageNo = newPageNo;//数据加载成功

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

$(document).ready(function() {
	
	console.log(url_hotwords_g);
	
	if(jsonData == '') {
		$.getJSON(url_hotwords_g, {num: 10}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
	if(keyword != '') {
		$keyword.val(keyword);
		hasSearchBoxTips = false;
		onGetPage(1, $('body'));
	}
	
	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	// 输入框点击时取消提示语
	$input.focus(function(evtO) {
		if (hasSearchBoxTips) { $(this).val(''); hasSearchBoxTips = false; }
	});

	// 提交按钮
	$form.submit(function(evtO) {
		if (hasSearchBoxTips) return false; //
		
		onGetPage(1, $('body'));
		
		return false;
	});
}); // $$$ $(document).ready

function keywordClick(obj) {
	searchType = 1; //hotword search
	console.log($(obj).html());
	$keyword.val($(obj).html());
	hasSearchBoxTips = false;
	
	onGetPage(1, $('body'));
	
	return false;
}
