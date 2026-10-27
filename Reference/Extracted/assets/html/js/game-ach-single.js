var isDetailStatus = true;

var achId = p_util.getUrlQueryPara('achId');
var playerType = p_util.getUrlQueryPara('playerType');
var timeType = p_util.getUrlQueryPara('timeType');

var PLAYER_TYPE_ID_ALL = 0; //热门榜单
var PLAYER_TYPE_ID_FRIEND = 1; //新品榜单
var PLAYER_TYPE_ID_NEAR = 2; //评分榜单
var PLAYER_TYPE = [PLAYER_TYPE_ID_ALL, PLAYER_TYPE_ID_FRIEND, PLAYER_TYPE_ID_NEAR]; //所有、好友、附近
var TIME_TYPE = [1, 2, 0]; //本日、本周、所有
var myGpsX = '';
var myGpsY = '';
console.log(achId);

var url_scoreType_g = p_util.getUrl("ad/j_achRank.g", g_refresh_interval);
var tmplScoreType = tmpl('tmpl-single-rank');
var $ul = $(".player-single-rank-list");
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

function loadJson(pageNo, jsonObj, stat) {
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	
	myGpsX = jsonObj.extra.myGpsX;
	myGpsY = jsonObj.extra.myGpsY;
	
	$ul.empty();
	$.each(jsonObj.value, function(key, val) {
		if(val.uin != jsonObj.myuin) {
			var _html = tmplScoreType(val);
			console.log(_html);
			$ul.append(_html);
		}
	});

	// 隐藏空数据提示框
	if(jsonObj.value.length == 0 && pageNo==1) {
		$warn.css('display', 'block');
	}
	else {
		$warn.css('display', 'none');
	};
	
	
	//数据加载成功
	g_pageTool.updateBtn(pageNo, jsonObj.more==1);
	loadComplete();
}

// ready
$(document).ready(function() {

	
	/**
	 * 获取更多
	 */
	function onGetPage(newPageNo, btn) {
		loading(btn);
		if(jsonData == '' || newPageNo>1) {
			$.getJSON(url_scoreType_g, {cpId: cpId, gameId: gameId, sid: sid, achId: achId, playerType: playerType,pageno: newPageNo, timeType: timeType }, 
					function(jsonObj, stat){
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

	//地域位置切换事件; 第一行； 所有好友 附近； PLAYER_TYPE
	$('#main-nav li').click(function(evtO) {
		
		$this = $(this);//当前点击元素
		console.log($this.index());
		playerType = PLAYER_TYPE[$this.index()];
		jsonData = '';
		$('#main-nav li a.'+onCls).rmCls(onCls);
		$this.children().addCls(onCls);
		
		onGetPage(1, $('body'));
		return false;
	});
	
}); // $$$ $(document).ready
