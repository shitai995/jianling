
// csnt
var PLAYER_TYPE_ID_ALL = 0; //热门榜单
var PLAYER_TYPE_ID_FRIEND = 1; //新品榜单
var PLAYER_TYPE_ID_NEAR = 2; //评分榜单
var PLAYER_TYPE_ARRAY = [PLAYER_TYPE_ID_ALL, PLAYER_TYPE_ID_FRIEND, PLAYER_TYPE_ID_NEAR];
var myGpsX = '';
var myGpsY = '';

var tmplCategory = tmpl('tmpl-player-list');
$ul = $(".friends-list");
var $warn = $(".nodata-wrapper");
var typeId = p_util.getUrlQueryPara('typeId', PLAYER_TYPE_ID_ALL);
console.log(typeId);
var url_rank_g = p_util.getUrl("ad/j_player.g", g_refresh_interval);

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

$(document).ready(function() {

	
	/**
	 * 获取更多游戏
	 */
	function onGetPage(newPageNo, btn) {
		loading(btn);
		
		$.getJSON(url_rank_g, { pageno: newPageNo, typeId: typeId}, function(jsonObj, stat) {
				
				if(p_util.isPreLoad()) {
					return;
				}
				
				window.g_resBaseUrl = jsonObj.resourceUrl;
				var dataCnt = 0;
				if (jsonObj && jsonObj.value) {
					$ul.html('');
					myGpsX = jsonObj.extra.myGpsX;
					myGpsY = jsonObj.extra.myGpsY;
					$.each(jsonObj.value, function(index, val){
						if(jsonObj.myuin != val.uin) {
							var _html = tmplCategory(val);
							console.log(_html);
							$ul.append(_html);
							dataCnt++;
						}

					});
				}
				
				// 隐藏空数据提示框
				if(dataCnt == 0 && newPageNo==2) {
					$warn.css('display', 'block');
				}
				else {
					$warn.css('display', 'none');
				};

				
				//数据加载成功
				g_pageTool.updateBtn(newPageNo, jsonObj.more==1);
				loadComplete();
				//*/

			}
		);

		
		
	} // $$$ function onGetMore(eventObj) {

	// 初始化分页工具
	g_pageTool.init(onGetPage);
	onGetPage(1, $('body'));
	
//	if(typeId == PLAYER_TYPE_ID_FRIEND)
//	{
//		$('#main-nav li a.'+onCls).rmCls(onCls);
///		$('#main-nav li:nth-child(2) a').addCls(onCls);
//	}
	
	//最上方切换榜单事件
	$('#main-nav li').click(function(evtO) {
		$this = $(this);
		console.log($this.index());
		typeId = PLAYER_TYPE_ARRAY[$this.index()];
		
		$('#main-nav li a.'+onCls).rmCls(onCls);
		$this.children().addCls(onCls);
		onGetPage(1, $('body'));
		
		return false;
	}); // $$$ 最上方切换榜单事件
	
	//$(document).ajaxError(function() {p_pkg.showMsg('网络异常，请稍后再试！'); });
	
	
}); // $$$ $(document).ready


