/**
 * 
 */
var RANK_ARRAY = [HOT_RANK_ID, NEW_RANK_ID, REMARK_RANK_ID];

var typeid = p_util.getUrlQueryPara('typeid');
var rankid = p_util.getUrlQueryPara('rankid', HOT_RANK_ID);
console.log(typeid, rankid);

var url_rank_g = p_util.getUrl("ad/j_rank.g", 60*24);
var tmplHotGame = $.tmpl('tmpl-game');
var $type = $('#type');
var $hotGameCategoryUl = $.q("#w-game-list ul");

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
		$.empty($hotGameCategoryUl);
		$type.html(jsonObj.extra.typeName + "类");
		$.each(jsonObj.value, function(index, val){
			var _html = tmplHotGame(val);
			console.log(_html);
			$hotGameCategoryUl.appendChild($.docFrag(_html));
		});
	}

	//数据加载成功
	g_pageTool.updateBtn(1, jsonObj.more==1);
	loadComplete();
}


/**
 * 获取分页
 */
function onGetPage(newPageNo, btn) {
	loading(btn);
	
	$.getJSON(url_rank_g, {pageno: newPageNo, typeid: typeid, rankid: rankid}, function(jsonObj, stat) {

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
	
	console.log(url_rank_g);
	
	if(jsonData == '') {
		$.getJSON(url_rank_g, { typeid: typeid, rankid: rankid}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	
	//最上方切换榜单事件
	$('.sec-nav li').click(function(evtO) {
		$this = $(this);
		console.log($this.index());
		rankid = RANK_ARRAY[$this.index()];
		
		$('.sec-nav li a.cur').rmCls('cur');
		$this.children().addCls('cur');
		
		onGetPage(1, $('body'));
		
		return false;
	}); // $$$ 最上方切换榜单事件
	
	
	if (rankid == NEW_RANK_ID) {
		$('.main-nav li:nth-child(2)').click();
	}
	
	// 初始化分页工具
	g_pageTool.init(onGetPage);

}); // $$$ $(document).ready

