


var url_inviteList_g = p_util.getUrl("ad/j_inviteList.g", 0.01);
var url_postFriendRefuse_g = p_util.getUrl("ad/j_postFriendRefuse.g", 0.01);
var url_postFriendConfirm_g = p_util.getUrl("ad/j_postFriendConfirm.g", 0.01);





//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

var pageNo = 1;

var tmplList = tmpl('tmpl-notice-list');
$ul = $("ul.notice-list");

var $viewmore = $(".get-more");
var $viewmore_a = $(".get-more a");
var $warn = $(".nodata-wrapper");



function processEvent() { // 加载成功，处理请求
	
	
	
	$('.w-allow').click(function(evtO) {
		
		$li = $(this).parents('li');
		$form = $li.find('form');
		console.log($li.html());
		$.getJSON(url_postFriendConfirm_g,  {	sign: $form.find('#form-sign').val(),
												uin: $form.find('#form-uin').val(), 
												dateTime: $form.find('#form-dateTime').val()},
											function(jsonObj, stat) {
			console.log(jsonObj);
			if (jsonObj.result == '0') {
				$li.hide();
				
				if (p_pkg.isFromAndroidClient) {
					p_pkg.setData("isUpdate", "true");
				}
			}
			p_pkg.showMsg(jsonObj.msg);
		});
		return false;
	});
	
	
	
	$('.w-refuse').click(function(evtO) {
		
		$li = $(this).parents('li');
		$form = $li.find('form');
		console.log($li.html());
		
		$.getJSON(url_postFriendRefuse_g, {	sign: $form.find('#form-sign').val(),
											uin: $form.find('#form-uin').val(), 
											dateTime: $form.find('#form-dateTime').val()},
			function(jsonObj, stat) {
			console.log(jsonObj);
			if (jsonObj.result == '0') {
				$li.hide();
				
				if (p_pkg.isFromAndroidClient) {
					p_pkg.setData("isUpdate", "true");
				}
			}
			p_pkg.showMsg(jsonObj.msg);
		});
		return false;
	});
	
} // $$$ function processEvent() { // 加载成功，处理请求




$(document).ready(function() {
	
	

	
	/**
	 * 获取更多
	 */
	function onGetMore() {
		
		//处理viewmore
		$(".get-more").css('display', 'block');//先保证viewmore显示
		$viewmore_a.detach();
		$viewmore.append(g_lc.canvasElem);
		
		function loadJson(jsonObj, stat) {
			console.log('hi');
			
			if(p_util.isPreLoad()) {
				return;
			}
			
			window.g_resBaseUrl = jsonObj.resourceUrl;
			var dataCnt = 0;
			if (jsonObj && jsonObj.value) {
				$.each(jsonObj.value, function(index, val){
					var _html = tmplList(val); console.log(_html); $ul.append(_html);
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
			
			
			processEvent();// 加载成功，处理请求
			
			pageNo++;

		} // $$$ loadJson
		
		
		if(jsonData == '' || pageNo>1) {
			$.getJSON(url_inviteList_g, {pageno: pageNo}, loadJson);
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


