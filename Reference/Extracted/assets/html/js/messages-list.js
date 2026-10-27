


var url_mailList_g = p_util.getUrl("ad/j_mailList.g", g_refresh_interval);



//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

var pageNo = 1;

var tmplMsgList = tmpl('tmpl-messages-list');
$ul = $("ul.messages-list");

var $viewmore = $(".get-more");
var $viewmore_a = $(".get-more a");
var $warn = $(".nodata-wrapper");

//跳转到游戏介绍页
function goToMsg(fromUin, dateTime, msgId) {
	
	window.location.href = "message.html?sid="+sid+"&fromUin=" + fromUin + "&dateTime="  + dateTime + "&msgId=" + msgId ;
}

function loadJson(jsonObj, stat) {
	
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;

	if (jsonObj && jsonObj.value) {
		var dataCnt = 0;
		$.each(jsonObj.value, function(index, val){
			var _html = tmplMsgList(val);
			console.log(_html);
			$ul.append(_html);
			dataCnt++;
		});
	} // $$$ if

	// 隐藏空数据提示框
	if(dataCnt == 0 && pageNo==1) {
		$warn.css('display', 'block');
	}
	else {
		$warn.css('display', 'none');
	};
	
	//数据处理完毕，开始处理viemore
	$(g_lc.canvasElem).detach();
	if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
	else { $(".get-more").css('display', 'none');}//隐藏viewmore框
	
	pageNo++;
}

$(document).ready(function() {
	
	/**
	 * 获取更多
	 */
	function onGetMore() {
		
		//处理viewmore
		$(".get-more").css('display', 'block');//先保证viewmore显示
		$viewmore_a.detach();
		$viewmore.append(g_lc.canvasElem);
		
		if(jsonData == '' || pageNo>1) {
			$.getJSON(url_mailList_g, {pageno: pageNo}, loadJson);
		} else {
			console.log('load json: '+ jsonData);
			loadJson(jsonData);
		}

		
	} // $$$ function onGetMore(eventObj) {

	//$.getJSON(url_rank_g, {pageno: pageNo++, typeid: typeId, rankid: rankId},onGetMore); // $$$ $.getJSON

	$('.get-more a').click(onGetMore);
	onGetMore();
	
	
	
	
	
	
	
	
	
	
	
	
	$(document).ajaxError(function() {p_pkg.showMsg('网络异常，请稍后再试！'); });
	
	
}); // $$$ $(document).ready


