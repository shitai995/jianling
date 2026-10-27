var isDetailStatus = true;

var uin = p_util.getUrlQueryPara('uin');
console.log(uin);

var curCpId = p_pkg.getCpId();
var curGameId = p_pkg.getGameId();
console.log(curCpId);
console.log(curGameId);
var pagesize = p_util.getUrlQueryPara('pagesize', 15);
console.log(pagesize);

var url_profile_g = p_util.getUrl("ad/j_profile.g", g_refresh_interval);
var url_feed_g = p_util.getUrl("ad/j_feedUser.g", g_refresh_interval);

var url_postSign_g = p_util.getUrlEx("ad/j_postSign.g", curCpId, curGameId);
var url_postMail_g = p_util.getUrlEx("ad/j_postMail.g", curCpId, curGameId);
var url_postInvite_g = p_util.getUrlEx("ad/j_postInvite.g", curCpId, curGameId);//添加好友
var url_postDel_g = p_util.getUrlEx("ad/j_postFriendDel.g", curCpId, curGameId);//删除好友

cpId = p_pkg.getCpId();
gameId = p_pkg.getGameId();
console.log("cur cpId:"+cpId);
console.log("cur gameId:"+gameId);

var isSelf = true;
var tmplFeed = tmpl('tmpl-feed');
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
	if(dataCnt == 0 && g_pageTool.pageNo == 1) {
		$warn.css('display', 'block');
	}
	else {
		$warn.css('display', 'none');
	};
	
	$(g_lc.canvasElem).detach();
	
}

/**
 * 获取更多feed
 */
function onGetPage(newPageNo, btn) {
	loading(btn);
	$.getJSON(url_feed_g, { uin: uin, pageno: newPageNo, pagesize: pagesize}, function(jsonObj, stat) {

		// 加载数据
		window.g_resBaseUrl = jsonObj.resourceUrl;
		loadFeed(jsonObj.value, jsonObj.more);
		
		// 数据加载成功
		g_pageTool.updateBtn(newPageNo, jsonObj.more);
		loadComplete();
	});
}

// ready
$(document).ready(function() {
	
	$.getJSON(url_profile_g, {uin: uin}, function(jsonObj, stat) {
		
		window.g_resBaseUrl = jsonObj.resourceUrl;
		console.log('g_resBaseUrl:'+window.g_resBaseUrl);

		$(".profile").html($.tmpl('tmpl-profile', jsonObj.value));
		$(".profile-sign").html($.tmpl('tmpl-profile-sign', jsonObj.value));
		$(".game-operation").html($.tmpl('tmpl-profile-game-info', jsonObj.value));
		$("#profile-my-info").html($.tmpl('tmpl-profile-my-info', jsonObj.value));

		// 初始化分页工具
		g_pageTool.init(onGetPage);
		
		onGetPage(1,$('body'));
		
		loadComplete();
		
		$('#update-sign').click(function(evtO) {
			$(this).attr('disabled', 'true');
			$.getJSON(url_postSign_g, {content: $.id('content').value}, function(jsonObj, stat) {
			//$.getJSON(url_postSign_g, {content: 'hi'}, function(jsonObj, stat) {
				console.log(jsonObj);
				p_pkg.showMsg(jsonObj.msg);
			});
			
			
			if (p_pkg.isFromAndroidClient) {
				p_pkg.setData("isUpdate", "true");
			}
			
			return false;
		});

		$('#content').keydown(function(evtO) {//输入
			//alert('keyd');
			$('#update-sign').rmAttr('disabled');
		});
		
	});
	
}); // $$$ $(document).ready
