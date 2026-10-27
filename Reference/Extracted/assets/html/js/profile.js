var isDetailStatus = true;

var uin = p_util.getUrlQueryPara('uin');
console.log(uin);
var act = p_util.getUrlQueryPara('act');
console.log(act);
var sign = p_util.getUrlQueryPara('sign');
console.log(sign);
var dateTime = p_util.getUrlQueryPara('dateTime');
console.log(dateTime);
var pagesize = p_util.getUrlQueryPara('pagesize', 15);
console.log(pagesize);

var curCpId = p_pkg.getCpId();
var curGameId = p_pkg.getGameId();
console.log(curCpId);
console.log(curGameId);

var url_profile_g = p_util.getUrl("ad/j_profile.g", g_refresh_interval);
var url_feed_g = p_util.getUrl("ad/j_feedUser.g", g_refresh_interval);

var url_postFriendRefuse_g = p_util.getUrl("ad/j_postFriendRefuse.g"); //拒绝好友请求
var url_postFriendConfirm_g = p_util.getUrl("ad/j_postFriendConfirm.g"); //接受好友请求
var url_postSign_g = p_util.getUrlEx("ad/j_postSign.g", curCpId, curGameId); //修改签名
var url_postMail_g = p_util.getUrlEx("ad/j_postMail.g", curCpId, curGameId); //发送好友消息
var url_postInvite_g = p_util.getUrlEx("ad/j_postInvite.g", curCpId, curGameId);//添加好友
var url_postDel_g = p_util.getUrlEx("ad/j_postFriendDel.g", curCpId, curGameId);//删除好友

var isSelf = false;

cpId = p_pkg.getCpId();
gameId = p_pkg.getGameId();
console.log("cur cpId:"+cpId);
console.log("cur gameId:"+gameId);

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
			if(val.content!='' ) {
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
}

/**
 * 获取更多feed
 */
function onGetPage(newPageNo, btn) {
	loading(btn);
	
	$.getJSON(url_feed_g, { uin: uin, pageno: newPageNo, pagesize: pagesize}, function(jsonObj, stat) {
		
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
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	console.log(window.g_resBaseUrl);
	
	$(".profile").html(tmpl('tmpl-profile', jsonObj.value));
	$(".profile-sign").html(tmpl('tmpl-profile-sign', jsonObj.value));
	$(".profile-sign").after(tmpl('tmpl-profile-message', jsonObj.value));
	$(".profile-sign").after(tmpl('tmpl-friend-request', jsonObj.value));
	$(".game-operation").html(tmpl('tmpl-profile-game-info', jsonObj.value));
	
	if(jsonObj.value.relaType == 'self') {
		isSelf = true;
		$("#profile-my-info").html(tmpl('tmpl-profile-my-info', jsonObj.value));
		$("#profile-del-friend").detach();
		$("#profile-add-friend").detach();
		$("#profile-add-request").detach();
		
		p_pkg.SetMessageNum(jsonObj.value.mailUnRead);
	}else if(jsonObj.value.relaType == 'friend'){
		$("#profile-my-info").detach();
		$(".profile-wrapper").addCls("profile-wrapper-other");
		$(".profile-wrapper-other").rmCls("profile-wrapper");
		$("#profile-add-friend").detach();
		$("#profile-add-request").detach();
		$("#profile-del-friend").show();
		
		//$(".profile-wrapper").att("class","profile-wrapper-other");
	}else{
		$("#profile-my-info").detach();
		$("#profile-del-friend").detach();
		$(".profile-wrapper").addCls("profile-wrapper-stranger");
		$(".profile-wrapper-stranger").rmCls("profile-wrapper");
		//$(".profile-wrapper").att("class","profile-wrapper-stranger");
		
		if(act == 'add'){
			$("#profile-add-request").show();
			$("#profile-add-friend").detach();
		}
		else {
			$("#profile-add-friend").show();
			$("#profile-add-request").detach();
		}
	}
	
	console.log('#update-sign: '+ $('#update-sign').html());
	$('#update-sign').click(function(evtO) {
		
		console.log('$(#update-sign).click');
		$(this).attr('disabled', 'true');
		$.getJSON(url_postSign_g, { content: $('#content').val()} , function(jsonObj, stat) {
			console.log(jsonObj);
			p_pkg.showMsg(jsonObj.msg);
			
			if (p_pkg.isFromAndroidClient) {
				p_pkg.setData("isUpdate", "true");
			}
		});
		
		$(this).attr('disabled', 'false');
		
		return false;
	});
	
	$('#send-msg').click(function(evtO) {
		var $msgBox = $('#msg-content');
		if($msgBox.val() == '') {
			p_pkg.showMsg('消息不能为空');
			return false;
		}
		
		if($msgBox.val().length > 100) {
			p_pkg.showMsg('消息不能超过100个字符');
			return false;
		}
		
		$.getJSON(url_postMail_g, {content: $msgBox.val(), uin: uin}, function(jsonObj, stat) {
			console.log(jsonObj);
			p_pkg.showMsg(jsonObj.msg);
			
			//发送成功，清空文本
			if(jsonObj.result == 0) {
				$msgBox.val('');
			}
		});
		
		return false;
	});	
	
	$('#profile-accept-friend').click(function(evtO) {
		
		$section = $('#profile-add-request');
		$.getJSON(url_postFriendConfirm_g, {sign: $('#friend-request-form-sign').val(),
											uin: $('#friend-request-form-uin').val(),
											dateTime: $('#friend-request-form-dateTime').val()},
											function(jsonObj, stat) {
			console.log(jsonObj);
			if (jsonObj.result == '0') {
				$section.hide();
			}
			p_pkg.showMsg(jsonObj.msg);
			
			if (p_pkg.isFromAndroidClient) {
				p_pkg.setData("isUpdate", "true");
			}
		});
		
	});
	
	$('#profile-refuse-friend').click(function(evtO) {
		
		$section = $('#profile-add-request');
		
		$.getJSON(url_postFriendRefuse_g, {	sign: $('#friend-request-form-sign').val(),
											uin: $('#friend-request-form-uin').val(), 
											dateTime: $('#friend-request-form-dateTime').val()},
											function(jsonObj, stat) {
			console.log(jsonObj);
			if (jsonObj.result == '0') {
				$section.hide();
				
				if (p_pkg.isFromAndroidClient) {
					p_pkg.setData("isUpdate", "true");
				}
			}
			p_pkg.showMsg(jsonObj.msg);
		});
		return false;
		
	});
	
	$('#profile-add-friend').click(function(evtO) {

		$.getJSON(url_postInvite_g, {content: $('#content').val(), uins: uin}, function(jsonObj, stat) {
			console.log(jsonObj);
			if (0 == jsonObj.result) {
				$('#profile-add-friend').hide();
				
				if (p_pkg.isFromAndroidClient) {
					p_pkg.setData("isUpdate", "true");
				}
			}
			p_pkg.showMsg(jsonObj.msg);
		});
		
		return false;
		
	});
	
	$('#btn-profile-del-friend').click(function(evtO) {
		
		var ret = window.confirm('真的要删除该好友吗？');
		if(ret) {
			$.getJSON(url_postDel_g, {content: $('#content').val(), uin: uin}, function(jsonObj, stat) {
				console.log(jsonObj);
				if (0 == jsonObj.result) {
					$('#profile-del-friend').hide();
					
					if (p_pkg.isFromAndroidClient) {
						p_pkg.setData("isUpdate", "true");
					}
				}
				p_pkg.showMsg(jsonObj.msg);
			});
		}
		
		return false;
		
	});
	
	$('#content').keydown(function(evtO) {//输入
		//alert('keyd');
		$('#update-sign').rmAttr('disabled');
	});
	

	
	// 初始化分页工具
	g_pageTool.init(onGetPage);
	
	onGetPage(1,$('body'));
	
	loadComplete();
	
} // $$$ loadJson


// ready
$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_profile_g, {uin: uin}, loadJson);
		
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	

}); // $$$ $(document).ready
