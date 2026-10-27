var pageNo = 1;
var url_scoreType_g = p_util.getUrl("ad/j_achTypes.g", g_refresh_interval);
var resId = '';

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));



// ready
$(document).ready(function() {

	var tmplScoreType = tmpl('tmpl-ach-types');
	var $ul = $(".achievement-list");
	var $viewmore = $(".get-more");
	var $viewmore_a = $(".get-more a");
	
	function loadJson(jsonObj, stat) {
		if(p_util.isPreLoad()) {
			return;
		}
		window.g_resBaseUrl = jsonObj.resourceUrl;
		$(".achievement-title").prepend(jsonObj.extra.game.name);
		resId = jsonObj.extra.game.resId;
		
		if (jsonObj && jsonObj.value) {

			$.each(jsonObj.value, function(key, val) {
				var _html = tmplScoreType(val);
				console.log(_html);
				$ul.append(_html);
			});
			
			if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
			else { $(".get-more").css('display', 'none');}//隐藏viewmore框
		}
		
		
		$(g_lc.canvasElem).detach();//全屏detach
		
		pageNo++;
	}
	
	if(jsonData == '') {
		$.getJSON(url_scoreType_g, {cpId: cpId, gameId: gameId, pageno: pageNo}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}
	

	/**
	 * 获取更多游戏
	 */
	function onGetMore() {
		$(".get-more").css('display', 'block');//先保证viewmore显示
		$viewmore_a.detach();
		$viewmore.append(g_lc.canvasElem);
		
		$.getJSON(url_scoreType_g, {cpId: cpId, gameId: gameId, pageno: pageNo++}, function(jsonObj, stat) {

				window.g_resBaseUrl = jsonObj.resourceUrl;

				if (jsonObj && jsonObj.value) {

					$.each(jsonObj.value, function(key, val) {
						var _html = tmplScoreType(val);
						console.log(_html);
						$ul.append(_html);
					});
					
				}


				$(g_lc.canvasElem).detach();
				
				if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
				else { $(".get-more").css('display', 'none');}//隐藏viewmore框
				//*/

			}
		);

		
		
	} // $$$ function onGetMore(eventObj) {

	$('.get-more a').click(onGetMore);
	
	$(document).ajaxError(function() { p_pkg.showMsg('网络异常，请稍后再试！'); });
	
}); // $$$ $(document).ready
