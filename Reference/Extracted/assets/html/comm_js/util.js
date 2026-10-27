/**
 * util library
 * @author devinwang
 */


/**
 * create pkg
 */
var p_util = window.p_util || {};// create pkg



/**
 * debug console
 */
if(!window.console) window.console={};if(!window.console.log) window.console.log=function(){};

/**
 * 获取url中query parameter map, 并获存该map
 */
p_util.getUrlQueryParaMap = function() {
	p_util.__PARA_MAP_CACHE__ = p_util.__PARA_MAP_CACHE__ = {};
	var parts = window.location.search.replace(/[?&]+([^=&]+)=([^&]*)?/gi,
			function(s, key, value) {
				p_util.__PARA_MAP_CACHE__[key] = (value === undefined) ? "" : value;
			});
	return p_util.__PARA_MAP_CACHE__;
} // $$$ p_util.getUrlQueryParaMap

/**
 * 获取url中query parameter
 * 
 * @para paraName - parameter name
 * @para defVal - the default value, if paraName not exits.
 */
p_util.getUrlQueryPara = function(paraName, defVal) {
	var mapCache = p_util.getUrlQueryParaMap();
	if(undefined == defVal) defVal = "";
	return mapCache[paraName] === undefined ? defVal : mapCache[paraName];
}; // $$$ p_util.getUrlQueryPara


p_util.cookie = {
	get : function(name) {
		name = document.cookie.match(RegExp("(^| )" + name + "=([^;]*)(;|$)"));
		return !name ? "" : unescape(name[2]);
	},
	/**
	 * set cookie
	 * @para name - cookie name
	 * @para domain - cookie domain. current domain, if not given.
	 * @para path - cookie path. /, if not given.
	 * @para expires - expires. 单位是天，如果未给出，则保存一个月。
	 */
	set : function(name, value, domain, path, expires) {
		var date = new Date;
		date.setTime(date.getTime() + (expires ? 36E5 * expires : 2592E6));
		document.cookie = name + "=" + value 
		+ (domain ? "; domain=" + domain : "")
		+ "; path=" + (path ? path : "/") 
		+ "; expires=" + date.toGMTString()		
				
	},
	/**
	 * del the cookie
	 * @para name - cookie name
	 * @para domain - cookie domain. current domain, if not given.
	 * @para path - cookie path. /, if not given.
	 */
	del : function(name, domain, path) {
		document.cookie = name + "=" 
				+ (domain ? "; domain=" + domain : "")
				+ "; path=" + (path ? path : "/") 
				+ "; expires=Mon, 26 Jul 1997 05:00:00 GMT"
	},
	/**
	 * del all cookie in current domain and path /
	 */
	delAll : function() {
		var arr = document.cookie.match(RegExp("([^ ;][^;]*)(?=(=[^;]*)(;|$))",
				"gi"));
		for (n in arr)
			document.cookie = arr[n]
					+ "=; path=/; expires=Mon, 26 Jul 1997 05:00:00 GMT"
	}
};

