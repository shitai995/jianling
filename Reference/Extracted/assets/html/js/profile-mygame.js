var pageNo = 1;
var url_scoreType_g = p_util.getUrl("ad/j_userGame.g",  p_pkg.getData("isUpdate") == "true" ? 1 : g_refresh_interval);
var url_reportDown_g = p_util.getUrl("ad/j_down.g");

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div  style="position:absolute; left:50%; top:35%; margin-left:-20px; z-index: 999;"></div>').append(g_lc.canvasElem));

var $warn = $(".nodata-wrapper");

// ready
$(document).ready(function() {
	var tmplScoreType = tmpl('tmpl-ach-list');
	var $ul = $(".game-list");
	
	var $viewmore = $(".get-more");
	var $viewmore_a = $(".get-more a");
	
	function loadJson(jsonObj, stat) {
		
		if(p_util.isPreLoad()) {
			return;
		}
		
		window.g_resBaseUrl = jsonObj.resourceUrl;
		var dataCnt = 0;
		
		if (jsonObj && jsonObj.value) {

			$.each(jsonObj.value, function(key, val) {
				if(val.game.name != '') {
					var _html = tmplScoreType(val);
					console.log(_html);
					$ul.append(_html);
					dataCnt++;
				}
			});
			
			if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
			else { $(".get-more").css('display', 'none');}//隐藏viewmore框
		}
		
		// 隐藏空数据提示框
		if(dataCnt == 0 && pageNo==1) {
			$warn.css('display', 'block');
		}
		else {
			$warn.css('display', 'none');
		};
		
		$(g_lc.canvasElem).detach();//全屏detach
		
		pageNo++;
	}

	if(jsonData == '' || pageNo > 1) {
		$.getJSON(url_scoreType_g, {pageno: pageNo}, loadJson);
		
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
		
		$.getJSON(url_scoreType_g, {pageno: pageNo}, function(jsonObj, stat) {

				window.g_resBaseUrl = jsonObj.resourceUrl;

				if (jsonObj && jsonObj.value) {

					$.each(jsonObj.value, function(key, val) {
						if(val.game.name != '') {
							var _html = tmplScoreType(val);
							console.log(_html);
							$ul.append(_html);
						}
					});
					
				}


				$(g_lc.canvasElem).detach();
				
				if (jsonObj.more > 0) { $viewmore.append($viewmore_a); }
				else { $(".get-more").css('display', 'none');}//隐藏viewmore框
				//*/
				
				pageNo++;
			}
		);

		
		
	} // $$$ function onGetMore(eventObj) {

	$('.get-more a').click(onGetMore);
	
	$(document).ajaxError(function() {p_pkg.showMsg('网络异常，请稍后再试！'); });
	
}); // $$$ $(document).ready
