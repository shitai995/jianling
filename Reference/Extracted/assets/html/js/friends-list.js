
// csnt
var PLAYER_TYPE_ID_ALL = 0; //热门榜单
var PLAYER_TYPE_ID_FRIEND = 1; //新品榜单
var PLAYER_TYPE_ID_NEAR = 2; //评分榜单
var PLAYER_TYPE_ARRAY = [PLAYER_TYPE_ID_FRIEND, PLAYER_TYPE_ID_NEAR];

var typeId = p_util.getUrlQueryPara('typeId', PLAYER_TYPE_ID_FRIEND);
console.log(typeId);

cpId = -1;
gameId = -1;
var url_rank_g = p_util.getUrl("ad/j_player.g",  g_refresh_interval);
var myGpsX = '';
var myGpsY = '';

var tmplCategory = tmpl('tmpl-player-list');
var $ul = $(".friends-list");
var tmplWarn = tmpl('tmpl-nodata');
var $warn = $(".nodata-wrapper");
var $viewmore = $(".get-more");
var $viewmore_a = $(".get-more a");

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


function loadJson(newPageNo, jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}
	window.g_resBaseUrl = jsonObj.resourceUrl;
	var dataCnt = 0;
	if (jsonObj && jsonObj.value) {
		$ul.html('');
		$.each(jsonObj.value, function(index, val){
			if(jsonObj.myuin != val.uin) {
				dataCnt++;
				myGpsX = jsonObj.extra.myGpsX;
				myGpsY = jsonObj.extra.myGpsY;
				var _html = tmplCategory(val);
				console.log(_html);
				$ul.append(_html);
			}
		});
	}

	// 隐藏空数据提示框
	if(dataCnt == 0 && g_pageTool.newPageNo == 1) {
		var _warn_html = tmplWarn(jsonObj);
		console.log(_warn_html);
		$warn.html(_warn_html);
		$warn.css('display', 'block');
	}
	else {
		$warn.css('display', 'none');
	};

	//数据加载成功
	g_pageTool.updateBtn(newPageNo, jsonObj.more==1);
	loadComplete();
}

$(document).ready(function() {

	/**
	 * 获取更多游戏
	 */
	function onGetPage(newPageNo, btn) {
		loading(btn);

		if(jsonData == '' || newPageNo > 1) {
			$.getJSON(url_rank_g, { pageno: newPageNo, typeId: typeId},  function(jsonObj, stat){
				loadJson(newPageNo, jsonObj, stat);
			});
			
		} else {
			console.log('load json: '+ jsonData);
			loadJson(1, jsonData);
		}
		
	} // $$$ function onGetPage(eventObj) {

	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	onGetPage(1, $('body'));

	
	if(typeId == PLAYER_TYPE_ID_FRIEND)
	{
		$('#main-nav li a.'+onCls).rmCls(onCls);
		$('#main-nav li:nth-child(1) a').addCls(onCls);
	}
	else
	{
		$('#main-nav li a.'+onCls).rmCls(onCls);
		$('#main-nav li:nth-child(2) a').addCls(onCls);
	}
	
	
	$("#friend").click(function(e) {
		e.preventDefault ? e.preventDefault() : e.returnValue = false;
		
		pageNo = 1;
		typeId = PLAYER_TYPE_ID_FRIEND;
		jsonData = '';
		$("#friend a").addCls(onCls);
		$("#nearby a").rmCls(onCls);
		
		$ul.html('');
		onGetPage(1, $('body'));
		
	});
	
	
	$("#nearby").click(function(e) {
		e.preventDefault ? e.preventDefault() : e.returnValue = false;
		pageNo = 1;
		typeId = PLAYER_TYPE_ID_NEAR;
		jsonData = '';
		$("#friend a").rmCls(onCls);
		$("#nearby a").addCls(onCls);
		
		$ul.html('');
		onGetPage(1, $('body'));
		
		return false;
	});
	
	
	//$(document).ajaxError(function() { alert('网络异常，请稍后再试！'); });
	
	
}); // $$$ $(document).ready




