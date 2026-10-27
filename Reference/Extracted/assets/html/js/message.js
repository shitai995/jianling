var hasSearchBoxTips = true;


var curCpId = p_pkg.getCpId();
var curGameId = p_pkg.getGameId();
var url_mailContent_g = p_util.getUrl("/ad/j_mailContent.g", g_refresh_interval);
var url_postMail_g = p_util.getUrlEx("ad/j_reMail.g", curCpId, curGameId);
var url_delMail_g = p_util.getUrlEx("ad/j_delMail.g", curCpId, curGameId);


var fromUin = p_util.getUrlQueryPara('fromUin');
var dateTime = p_util.getUrlQueryPara('dateTime');
var msgId = p_util.getUrlQueryPara('msgId');


//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div  style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));

var pageNo = 1;

var tmplList = tmpl('tmpl-notice-list');
$dt = $("dt");



function processEvent() {//process event
	
	//发送消息
	$('#send-msg').click(function(evtO) {
		var cnt = $('#content').val();
		if (cnt.length > 250) {
			p_pkg.showMsg('字数超过250,请删除部分内容')
			return false;
		}
		$(this).attr('disabled', true);
		
		$.getJSON(url_postMail_g, {content: cnt, uin: fromUin}, function(jsonObj, stat) {
			console.log(jsonObj);
			p_pkg.showMsg(jsonObj.msg);
			if(jsonObj.result == 0) {
				$('#content').val('');
			}
		});
		
		return false;
	});	
	
	
	//删除消息
	$('#del-msg').click(function(evtO) {
		
		$.getJSON(url_delMail_g, {fromUin: fromUin, dateTime: dateTime, msgId: msgId}, function(jsonObj, stat) {
			console.log(jsonObj);
			p_pkg.showMsg(jsonObj.msg);
			if (0 == jsonObj.result) {
				p_pkg.setData("isUpdate", "true");
			}
		});
		
		return false;
	});	
	
	
	//有键输入动作，enable button
	$('#content').keydown(function(evtO) {//输入
		//alert('keyd');
		$('input:button').removeAttr('disabled');
	});
	
} // $$$ function processEvent() {//process event




//点击时取消提示语
$('#content').focus(function(evtO) {
	if (hasSearchBoxTips) { $(this).val(''); hasSearchBoxTips = false; }
});



$(document).ready(function() {
	
	
	function loadJson(jsonObj, stat) {
		if(p_util.isPreLoad()) {
			return;
		}
		
		if (jsonObj.result != '0') return false;
			
			window.g_resBaseUrl = jsonObj.resourceUrl;
			
			
			$dt.empty();
			
			var _html = tmplList(jsonObj.value.msg); console.log(_html); $dt.append(_html);
			p_pkg.setData("isUpdate", "true");

			processEvent();
			
			// 未读状态变化
			if(jsonObj.value.msg.readFlag == 1) {
				p_pkg.setData("isUpdate", "true");
			}
			
			$(g_lc.canvasElem).detach();//全屏detach
			
	} // $$$ $.getJSON
	
	if(jsonData == '') {
		$.getJSON(url_mailContent_g, {fromUin: fromUin, dateTime: dateTime, msgId: msgId}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	

		
		

	//$.getJSON(url_rank_g, {pageno: pageNo++, typeid: typeId, rankid: rankId},onGetMore); // $$$ $.getJSON

	

	
	
	
	
	$(document).ajaxError(function() {p_pkg.showMsg('网络异常，请稍后再试！'); });
	
	
}); // $$$ $(document).ready


