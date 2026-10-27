/*!
 * SIMPLE js framework
 * @author wangdingwei
 */




(function( window, undefined ) {


	/**
	 * SIMPLE.aStaticFunc = function() {}; // static method, MUST be called in the form of $.aStaticFunc
	 * $('str') is instance of ElemWrapper. 
	 * $('str').anInstMethod; first, lookup the instance; second, lookup the constructor's prototype (ElemWrapper.prototype); and so on ....
	 */
	
	// Use the correct document accordingly with window argument (sandbox)
	var document = window.document;
	
	/** debug console */
	if(!window.console) window.console={};if(!window.console.log) window.console.log=function(){};

	
	
	
	
	
	/************************************** selector engine *****************************************
	 element selector engine
	 not implemented
	 ************************************************************************************************/
	
	
	
	/**
	 * SIMPLE framework
	 */
	var SIMPLE = (function() { 
		var SIMPLE = function(selector, context) {return new ElemWrapper(selector, context); };
		return SIMPLE;
	})();
	
	
	
	
	
	 
	/**
	 * element list class
	 * !!! SIMPLE('str') is an instance of this class
	 * 
	 * hold Element
	 * manipulate element
	 * 
	 * @param selector - selector to selector element
	 */
	function ElemWrapper(selector, context) {
		this.selector = selector;
		this.context = context;
		this.length = 0;
		// doc = (context ? context.ownerDocument || context : document);
		
		var match;

		// Handle $(""), $(null), or $(undefined)
		if ( !selector ) { return this; }

		// Handle $(DOMElement)
		if ( selector.nodeType ) {
			this.context = this[0] = selector;
			this.length = 1;
			return this;
		}

		// The body element only exists once, optimize finding it
		if ( selector === "body" && !context && document.body ) {
			this.context = document;
			this[0] = document.body;
			this.selector = "body";
			this.length = 1;
			return this;
		}
		
		// string
		if ( typeof selector === "string" ) {
			selector.trim();
			var _doc = (context ? context.ownerDocument || context : document); // get document
			if (selector.length <= 15) {// #id .cls  tag <tag>  //they should less than 15
				//  /^(?:#([\w\d_-]+)|\.([\w\d_-]+)|(\w+)|<(\w+)>)$/.exec('.cls')
				//  [".cls", undefined, "cls", undefined, undefined]
				
				// id .cls tag <tag>
				match = /^(?:#([\w\d_-]+)|\.([\w\d_-]+)|(\w+)|<(\w+)>)$/.exec(selector); 
				
				if (match && match[1]) { //#id
					elem = document.getElementById( match[1] );
					if ( elem && elem.parentNode ) {
						this[0] = elem;
						this.length = 1;
					}
					return this;
				} // $$$ #id
				else if (match && match[2]) { //.cls
					var _ctx = null;
					if (context && context.getElementsByClassName) { _ctx = context; }
					else if (_doc.getElementsByClassName) { _ctx = _doc; }
		
					if (null != _ctx) { // has process
						this.addElems(_ctx.getElementsByClassName(match[2]));
						return this;	
					}
					
					// could not process class, continue
				} //$$$ .csl
				else if (match && match[3]) { //tag //getElementsByTagName MUST exist

					var _elemList;
					if (context && context.getElementsByTagName) { _elemList = context.getElementsByTagName(match[3]); }
					else { _elemList = _doc.getElementsByTagName(match[3]); }
					
					this.addElems(_elemList);
					return this;
					
				} // $$$ tag
				else if (match && match[4]) { //create <tag>
					this[0] = _doc.createElement(match[4]);
					this.length = 1;
					return this;
				}
			} // $$$ if (selector.length <= 15) {// #id .cls  tag <tag>  //they should less than 15
			
			
			//now html document fragment OR css selector(contain '<')
			if (selector.indexOf('<') >= 0) { // selector is html, create elem
				var divE = _doc.createElement("div");
				divE.innerHTML = selector;
				this.addElems(divE.childNodes);
				return this;
			}
			else { // css selector
				if(context && context.querySelectorAll) {// if context exist, use elem.querySelectorAll OR document.querySelectorAll
					try {
					this.addElems(context.querySelectorAll(selector)); //querySelectorAll my throw an error, if css selector is invalid
					} catch (err) {console.log("SIMPLE err: " + "context.querySelectorAll" + "|" + selector);}
					return this;
				}
				else if(_doc.querySelectorAll) {// use document.querySelectorAll
					try {
					this.addElems(_doc.querySelectorAll(selector)); //querySelectorAll my throw an error, if css selector is invalid
					} catch (err) {console.log("SIMPLE err: " + "document.querySelectorAll" + "|" + selector);}
					return this;
				}
				else {// err: can not process css selector
					var _msg = 'SIMPLE err: css selector is not supported!!!';
					alert(_msg);
					throw _msg;
				}
				
			}
				
		} //$$$ if ( typeof selector === "string" ) {
		else if(selector instanceof ElemWrapper) { //already SIMPLE // else if(selector.constructor === ElemWrapper) {
			return selector; // already SIMPLE retrun it;
		}
		else { // not a string
			var _msg = "SIMPLE err: selector `" + selector + "'is not a string!!!";
			alert(_msg);
			throw _msg;
		}
		
		
	} // $$$ function ElemWrapper(selector, context) {
	
	/** element size */
	ElemWrapper.prototype.size = function() { return this.length; };
	/**
	 * .each( function(index, Element){} );
	 *     the callback is fired in the context of the current DOM element, so the keyword this refers to the element.
	 */
	ElemWrapper.prototype.each =  function( callback ) {
		return SIMPLE.each( this, callback );
	};
	ElemWrapper.prototype.addElems = function(elems) {
		if (elems == null) {elems = [];} //if elems equals null, create an empty array
		if (elems.length == null) {elems = [elems];} //if elems is not an array, put itself to array
		for (var i = 0, e; e = elems[i]; i++) { this[this.length + i] = e; } // if e is undefined, break loop
		this.length = this.length + elems.length;
		return this;
	};
	/*
	ElemWrapper.prototype.append = function(child) {
		domManip(this, child, function(elemMy, elemSel) {
			if (elemMy.nodeType === 1) {SIMPLE.append(elemMy, elemSel);}
		});
		return this;
	};
	///*/
	
	
	//simple dom traversal, create a new ElemWrapper obj.
	var _S_DOM_TRAVERSL_F = ["next", "prev", "child", "children", "parent", "find"];
	for (var i in _S_DOM_TRAVERSL_F) { 
		// !!!!!!!!!!!!!!!!  important  !!!!!!!!!!!!!!!!!!
		// closure - make every func have it's own `name' var, and not to be changed. // SIMPLE.each is also OK
		(function(){ 
			var name = _S_DOM_TRAVERSL_F[i];
			ElemWrapper.prototype[name] = function(a1, a2) {
				var __ew = new ElemWrapper();
				this.each(function(iMy, elemMy) {
					if (elemMy.nodeType === 1) {__ew.addElems(SIMPLE[name](elemMy, a1, a2));}
				});
				return __ew;
			};
		})();
	} // $$$ for
	
	
	
	/** 
	 * simple 1 elem manipulate func.
	 * 
	 * When arguments.lengh 
	 * 	equal _S_RTN_VAL_LEN.funcName: return value
	 * 	equal _S_RTN_VAL_LEN.funcName+1: modify property, return chain(SIMPLE)
	 * 	otherwise: invalid argument
	 */
	var _S_DOM_1_ELEM_M_F_NOT_CHAIN = ["css", "attr", "html", "index", "val", "hasCls"];
	var _S_RTN_VAL_LEN = {css:1, attr:1, html:0, index:0, val:0, hasCls:1};
	for (var i in _S_DOM_1_ELEM_M_F_NOT_CHAIN) { 
		// !!!!!!!!!!!!!!!!  important  !!!!!!!!!!!!!!!!!!
		// closure - make every func have it's own `name' var, and not to be changed. // SIMPLE.each is also OK
		(function(){ 
			var name = _S_DOM_1_ELEM_M_F_NOT_CHAIN[i];
			ElemWrapper.prototype[name] = function(a1, a2) {
				if (arguments.length == _S_RTN_VAL_LEN[name]) { //return value
					if(this[0]) { return SIMPLE[name](this[0], a1); }
					else { return null; }
				}
				else if (arguments.length == _S_RTN_VAL_LEN[name]+1){// chain
					this.each(function(iMy, elemMy) {
						if (elemMy.nodeType === 1) {SIMPLE[name](elemMy, a1, a2);}
					});
					return this;
				}
				else { //invalid arguments
					var _msg = "SIMPLE err: `" + name + "'invalid argument";
					alert(_msg);
					throw _msg;
				}
			};
		})();
	} // $$$ for
	
	
	/** 
	 * simple 1 elem manipulate func 
	 * modify property. 
	 * return chain 
	 */
	var _S_DOM_1_ELEM_M_F = ["detach", "empty", "rmAttr", "rmCls", "addCls", "toggleCls", "show", "hide", "trigger"];
	for (var i in _S_DOM_1_ELEM_M_F) { // elem property
		// !!!!!!!!!!!!!!!!  important  !!!!!!!!!!!!!!!!!!
		// closure - make every func have it's own `name' var, and not to be changed. // SIMPLE.each is also OK
		(function(){ 
			var name = _S_DOM_1_ELEM_M_F[i];
			ElemWrapper.prototype[name] = function(a1, a2, a3) {
				this.each(function(iMy, elemMy) {
					if (elemMy.nodeType === 1) {SIMPLE[name](elemMy, a1, a2, a3);}
				});
				return this;
			};
		})();
	} // $$$ for
	
	
	/**
	 * simple two elem manipulate func. 
	 * return chain
	 */
	var _S_DOM_2_ELEM_M_F = ['append', 'prepend', 'before', 'after']; 
	for (var i in _S_DOM_2_ELEM_M_F) { // 2 elem manipulate
		// !!!!!!!!!!!!!!!!  important  !!!!!!!!!!!!!!!!!!
		// closure - make every func have it's own name, and not to be changed. // SIMPLE.each is also OK
		(function(){ 
			var name = _S_DOM_2_ELEM_M_F[i];
			ElemWrapper.prototype[name] = function(sel) {
				domManip42elem(this, sel, function(elemMy, elemSel) {
					if (elemMy.nodeType === 1) {SIMPLE[name](elemMy, elemSel);}
				});
				return this;
			};
		})();
	
	}
	/*
	for (var i in _S_DOM_A) {
		var name = _S_DOM_A[i]; // javascript var i local to a function, not {}
		// not work!!!
		ElemWrapper.prototype[name] = function(sel) {
			domManip(this, sel, function(elemMy, elemSel) {
				if (elemMy.nodeType === 1) {SIMPLE[name](elemMy, elemSel);} //!!!name will be changed.
			});
			return this;
		};
	
	}
	*/
	//*/
	/* each has not been declare here
	SIMPLE.each( ("append prepend before after").split(" "), function(index, name) {
		console.log(name);
		ElemWrapper.prototype[name] = function(sel) {
			console.log(name);
			domManip(this, sel, function(elemMy, elemSel) {
				console.log(name);
				if (elemMy.nodeType === 1) {SIMPLE[name](elemMy, elemSel);}
			});
			return this;
		};
	});
	*/
	
		

	
	
	
	// *** internal function ***
	
	/* 
	 * dom manipulate help func for 2 elem
	 */
	function domManip42elem($myS, sel, callback) {
		var $sel = SIMPLE(sel); // make sure selector is SIMPLE
		$myS.each(function(iMy, elemMy) {
			$sel.each(function(iSel, elemSel) {
				callback(elemMy, elemSel);
			});
		});
	}
	//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ selector engine $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
	
	
	/**************************** var declare **************************
	 * 
	 * 
	 *******************************************************************/
	SIMPLE.isDocReady = false;
	SIMPLE.docReadyCBList = [];
	SIMPLE.attrPropsFix = { //normalize/fix the name
			"for": "htmlFor",
			"class": "className",
			readonly: "readOnly",
			maxlength: "maxLength",
			cellspacing: "cellSpacing",
			rowspan: "rowSpan",
			colspan: "colSpan",
			tabindex: "tabIndex",
			usemap: "useMap",
			frameborder: "frameBorder"
		};
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	/********************************** core util *************************************
	 include:
	 1. string util
	 2. url util
	 **********************************************************************************/
	String.prototype.ltrim = function() { return this.replace(/^\s+/,""); }
	String.prototype.rtrim = function() { return this.replace(/\s+$/,""); }
	String.prototype.trim = String.prototype.trim || function() { return this.replace(/^\s+|\s+$/g,""); }
	/** get url parameter */
	SIMPLE.getUrlPara = function (name, defVal) {
		return (RegExp('(\\?|&)' + name+ '=([^&]*)').exec(location.search)||[,,undefined==defVal ? '' : defVal])[2];
	};
	/** parse json */
	SIMPLE.parseJSON = function( data ) {
		if ( typeof data !== "string" || !data ) { return null; }
		data.trim();

		// Make sure the incoming data is actual JSON // Logic borrowed from http://json.org/json2.js
		if ( /^[\],:{}\s]*$/.test(data.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@")
			.replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]")
			.replace(/(?:^|:|,)(?:\s*\[)+/g, "")) ) {
			// Try to use the native JSON parser first
			return window.JSON && window.JSON.parse ? window.JSON.parse( data ) : (new Function("return " + data))();
		} else { return null; }// invalid json
	};
	/** document ready callback handler */
	function DOMContentLoaded() {
		SIMPLE.isDocReady = true;
		for(var i = 0, cb; cb = SIMPLE.docReadyCBList[i]; i++) {cb();}
		document.addEventListener ? document.removeEventListener( "DOMContentLoaded", DOMContentLoaded, false ) : document.detachEvent( "onreadystatechange", DOMContentLoaded );
		
	}
	/** document ready */
	SIMPLE.ready = function(readyCB) {
		if (SIMPLE.isDocReady || document.readyState === "complete") {
			return setTimeout(readyCB, 1); // Handle it asynchronously to allow scripts the opportunity to delay ready
		}
		SIMPLE.docReadyCBList.push(readyCB);
		if (readyCB.length <= 1) {//only add the first time
			document.addEventListener ? document.addEventListener( "DOMContentLoaded", DOMContentLoaded, false ) : document.attachEvent("onreadystatechange", DOMContentLoaded);				
		}
	};
	SIMPLE.isElem = function(elem) {return elem && elem.nodeType && elem.nodeType === 1;}; // is an element node
	SIMPLE.each = function(object, callback) {
		if (!object) return;
		var name, i = 0, length = object.length;
		if (length !== undefined) { //array or array like
			for ( var value = object[0]; i < length && callback.call(value, i, value ) !== false; value = object[++i] ) {}
		} 
		else {//obj
			for ( name in object ) { if ( callback.call( object[ name ], name, object[ name ] ) === false ) { break; } }
		}
	} // SIMPLE.each = function(object, callback) {
	
	
	
	

	/********************************** type **************************************/
	// [[Class]] -> type pairs
	class2type = {};
	// Populate the class2type map
	SIMPLE.each("Boolean Number String Function Array Date RegExp Object".split(" "), function(i, name) {
		class2type[ "[object " + name + "]" ] = name.toLowerCase();
	});
	SIMPLE.type = function( obj ) {
		return obj == null ? String( obj ) : class2type[ Object.prototype.toString.call(obj) ] || "object";
	};
	SIMPLE.isFunction = function( obj ) { return jQuery.type(obj) === "function"; };
	SIMPLE.isArray = function (obj) { return Object.prototype.toString.call(obj) === '[object Array]'; } 
	SIMPLE.isEmptyObject = function( obj ) {
		for ( var name in obj ) { return false; }
		return true;
	};
	

	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	 
	
	
	/********************************** node util *************************************
	 nodeType
	 	Node.ELEMENT_NODE == 1
		Node.ATTRIBUTE_NODE == 2
		Node.TEXT_NODE == 3
		Node.CDATA_SECTION_NODE == 4
		Node.ENTITY_REFERENCE_NODE == 5
		Node.ENTITY_NODE == 6
		Node.PROCESSING_INSTRUCTION_NODE == 7
		Node.COMMENT_NODE == 8
		Node.DOCUMENT_NODE == 9
		Node.DOCUMENT_TYPE_NODE == 10
		Node.DOCUMENT_FRAGMENT_NODE == 11
		Node.NOTATION_NODE == 12
	 **********************************************************************************/
	
	SIMPLE.id = function(idN) { return document.getElementById(idN); }
	SIMPLE.tag = function(tagN) {return document.getElementsByTagName(tagN);}
	SIMPLE.q = function(sel) {return document.querySelector(sel);} // some browser does not have this function
	SIMPLE.qa = function(sel) {return document.querySelectorAll(sel);}
	/**
	 * createDocumentFragment
	 * @param html - html string, which will be the chile of DocumentFragment
	 */
	SIMPLE.docFrag = function(html, context) {
		var doc = (context ? (context.ownerDocument || context) : document);
		var docF = doc.createDocumentFragment(),
			divE = doc.createElement("div"),
			c;
		divE.innerHTML = html;
		while(c = divE.firstChild) {docF.appendChild(c);}
		//for (var i = 0, c; c = childNodes[i]; i++) { docF.appendChild(c); } //bug, iterate && change
		return docF;
	};
	SIMPLE.append = function(parent, child) { parent.appendChild(child); };
	SIMPLE.prepend = function(parent, child) { parent.insertBefore(child, parent.firstChild); };
	SIMPLE.before = function(elem, before) {elem.parentNode.insertBefore(before, elem);};
	SIMPLE.after = function(elem, after) {elem.nextSibling ? elem.parentNode.insertBefore(after, elem.nextSibling) : elem.appendChild(after);};
	SIMPLE.detach = function(elem) { elem && elem.parentNode && elem.parentNode.removeChild(elem); };
	SIMPLE.empty = function(elem) { while ( elem.firstChild ) { elem.removeChild( elem.firstChild );} 	};
	SIMPLE.html = function(elem, val) {return val != null ? elem.innerHTML = val : elem.innerHTML;}
	SIMPLE.attr = function(elem, name, val) {
		// don't get/set attributes on text, comment and attribute nodes
		if ( !elem || elem.nodeType === 3 || elem.nodeType === 8 || elem.nodeType === 2 ) { return undefined; }
		
		name = SIMPLE.attrPropsFix[name] || name;
		if(undefined === val) {return elem.getAttribute(name);}
		else if (null === val) {elem.removeAttribute(name);}
		else {elem.setAttribute(name, val);}
	};
	SIMPLE.rmAttr = function(elem, name) {elem.removeAttribute(name);};
	SIMPLE.val = function(elem, val) {
		if (!SIMPLE.isElem(elem)) return;
		// We need to handle select boxes special //refer jquery
		if (elem.nodeName == "select") {}
		if (elem.nodeName == "option") {}
		
		

		var name = elem.name;

		// 处理：可用并包含表单name的表单项(去掉 限制)
		//if (!elem.disabled && name) {
			var type = elem.type,
				value = elem.value; 
		
			switch (type) {
			case 'radio':
			case 'checkbox':
				//if (!elem.checked) {break;}
				if (null == val) {return elem.value;} //get
				else if (val === true || val === false) {elem.checked = val;} //boolean
				else {elem.checked = ( value == val || (SIMPLE.isArray(val) && val.indexOf(value)>=0) );}//value
				break;
			case 'textarea':
			case 'text':
			case 'password':
			case 'hidden':
				return val == null ? elem.value : elem.value = val;
			case 'select-one':
				if (val == null) {return elem.value;}
				//else process by 'select-multiple'
			case 'select-multiple': // multiple select
				var opts = elem.options,
					oLen = opts.length,
					selVals = new Array();
				for (oi = 0; oi < oLen; oi++) {
					var oItem = opts[oi], oVal = oItem.value;
					if (val == null) { if (oItem.selected) { selVals.push(oVal); } } //get
					else { oItem.selected = ( oVal == val || (SIMPLE.isArray(val) && val.indexOf(oVal)>=0) ); } //set
					
				} // $$$ for
			} // $$$ switch
		//} // $$$ if
		
		
	};
	SIMPLE.css = function(elem, name, val) {
		// Don't set styles on text and comment nodes
        if ( !elem || elem.nodeType === 3 || elem.nodeType === 8 || !elem.style ) { return; }
		name.replace(/-([a-z])/ig, function( all, letter ) { return letter.toUpperCase(); });// camelCase
		if(name === "float" || name === "cssFloat"){
    		if("" === "ie"){ name = "styleFloat"; } //ie
    		else{ name = "cssFloat"; }
    	}
		
		 if (typeof name == 'string' && val === undefined) {
			 //var style = window.getComputedStyle(elem, null);
			 // var style = document.defaultView.getComputedStyle(elem, null);
			 //the two is equivalent:
             return window.getComputedStyle ? window.getComputedStyle(elem, null).getPropertyValue(name) : elem.currentStyle[name];
         }
		 else {
			 elem.style[name] = val;
		 }
	};
	SIMPLE.rmCls = function(elem, cls) {
		if (!SIMPLE.isElem(elem)) {return;}
		var clsLst = (cls || "").split(/\s+/),
			len = clsLst.length;
		var className = (" " + elem.className + " ").replace(/[\n\t\r]/g, " ");
		for ( var i = 0; i < len; i++ ) {
			className = className.replace(new RegExp(" " + clsLst[i] + " ", "gi"), " ");
		}
		elem.className = className.trim();
	};
	SIMPLE.addCls = function(elem, cls) {
		if (!SIMPLE.isElem(elem)) {return;}
		var clsLst = (cls || "").split(/\s+/),
		len = clsLst.length;
		var className = (" " + elem.className + " ").replace(/[\n\t\r]/g, " ");
		for ( var i = 0; i < len; i++ ) {
			if ( className.indexOf( " " + clsLst[i] + " " ) < 0 ) {className += " " + clsLst[i];}
		}
		elem.className = className.trim();
	};
	SIMPLE.hasCls = function(elem, cls) {
		if (!SIMPLE.isElem(elem)) {return false;}
		var className = (" " + elem.className + " ").replace(/[\n\t\r]/g, " ");
		return className.indexOf( " " + cls + " " ) >= 0;
	};
	SIMPLE.toggleCls = function(elem, cls1, cls2) {
		if (!SIMPLE.isElem(elem)) {return;}
		if (SIMPLE.hasCls(elem, cls1)) {
			SIMPLE.rmCls(elem, cls1);
			SIMPLE.addCls(elem, cls2);
		}
		else {
			SIMPLE.addCls(elem, cls1);
			SIMPLE.rmCls(elem, cls2);
		}
	};
	SIMPLE.show = function(elem) {SIMPLE.css(elem, "display", "block");};
	SIMPLE.hide = function(elem) {SIMPLE.css(elem, "display", "none");}
	SIMPLE.index = function (elem) {
		if (!elem || elem.nodeType !=1) return undefined;
		var i = 0, c = elem.parentNode.firstChild;
		do { if (c === elem) return i; else if (c.nodeType === 1) i++;} while((c = c.nextSibling) != null);
	};
	SIMPLE.parent = function(elem) {if (SIMPLE.isElem(elem)) {return elem.parentNode} else {return null;}};
	SIMPLE.prev = function(elem) {
		if (!SIMPLE.isElem(elem)) { return null; }
		while (elem = elem.previousSibling) {if (SIMPLE.isElem(elem)) {return elem;}}; 
	};
	SIMPLE.next = function(elem) {
		if (!SIMPLE.isElem(elem)) { return null; }
		while (elem = elem.nextSibling) {if (SIMPLE.isElem(elem)) {return elem;}}; 
	};
	SIMPLE.children = function(elem) {
		if (!SIMPLE.isElem(elem)) { return null; }
		var elemA = new Array();
		for (var c, i=0; c = elem.childNodes[i]; i++) {
			if (SIMPLE.isElem(c)) {elemA.push(c);}
		}
		return elemA;
	};
	SIMPLE.find = function(elem, tag) {
		tag = tag.toUpperCase();
		if (!SIMPLE.isElem(elem)) { return null; }
		for (var c, i=0; c = elem.childNodes[i]; i++) {
			var name = c.tagName;
			if (SIMPLE.isElem(c) && name == tag) {
				return c;
			}
			else {
				var tmp = SIMPLE.find(c, tag);
				if (SIMPLE.isElem(tmp)) return tmp;
			}
		}

		return null;
	};
	/** 
	 * 1-indexed
	 * @param elem - Element
	 * @param index - child index
	 * @returns child elem of given index. null, if has no child. 
	 */
	SIMPLE.child = function(elem, index) {
		if (!SIMPLE.isElem(elem)) {return null;}
		if (typeof index !== "number" || index <= 0) index = 1;
		var elemI = 0,
			prevElem = null;
		for (var c, i=0; c = elem.childNodes[i]; i++) {
			if (SIMPLE.isElem(c)) {	elemI++; prevElem = c; }
			if (elemI == index) return c;
		}
		return prevElem; // return last elem, maybe null
		
	};
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	
	
	
	
	/******************************** event util *************************************/
	/* keyCode 值附表：
	 * ===============================
	 * 
	 * 1.主键盘区字母和数字键的键码值
	 * 按键 	键码
	 * 0 	48
	 * 1 	49
	 * 2 	50
	 * 3 	51
	 * 4 	52
	 * 5 	53
	 * 6 	54
	 * 7 	55
	 * 8 	56
	 * 9 	57
	 * 
	 * A 	65
	 * B 	66
	 * C 	67
	 * D 	68
	 * E 	69
	 * F 	70
	 * G 	71
	 * H 	72
	 * I 	73
	 * J 	74
	 * K 	75
	 * L 	76
	 * M 	77
	 * N 	78
	 * O 	79
	 * P 	80
	 * Q 	81
	 * R 	82
	 * S 	83
	 * T 	84
	 * U 	85
	 * V 	86
	 * W 	87
	 * X 	88
	 * Y 	89
	 * Z 	90
	 * 
	 * 
	 * 3.控制键键码值
	 * 按键			键码
	 * BackSpace	8
	 * Tab			9
	 * Clear		12
	 * Enter		13
	 * Shift		16
	 * Control		17
	 * Alt			18
	 * Cape Lock	20
	 * Esc			27
	 * Spacebar		32 
	 * Page Up		33
	 * Page Down	34
	 * End			35
	 * Home			36
	 * Left Arrow	37
	 * Up Arrow 	38
	 * Right Arrow	39
	 * Down Arrow	40
	 * Insert		45
	 * Delete		46
	 * 
	 * Num Lock		144
	 * 
	 * ;:			186
	 * =+			187
	 * ,<			188
	 * -_			189
	 * .>			190
	 * /?			191
	 * `~			192
	 * 
	 * [{			219
	 * \|			220
	 * }]			221
	 * ’"			222
	 * 
	 * 2.功能键键码值
	 * F1 	112
	 * F2 	113
	 * F3 	114
	 * F4 	115
	 * F5 	116
	 * F6 	117
	 * F7 	118
	 * F8 	119
	 * F9 	120
	 * F10 	121
	 * F11 	122
	 * F12 	123
	 * 
	 * 2.数字键盘上的键的键码值
	 * 按键 	键码
	 * 0 	96
	 * 1 	97
	 * 2 	98
	 * 3 	99
	 * 4 	100
	 * 5 	101
	 * 6 	102
	 * 7 	103
	 * 8 	104
	 * 9 	105
	 * 
	 * * 	106
	 * + 	107
	 * Enter108
	 * - 	109
	 * . 	110
	 * / 	111
	 * 
	 */
	/** bind */
	SIMPLE.bind = function(elem, eventType, handler, useCapture) {
		if ( !elem || elem.nodeType === 3 || elem.nodeType === 8 ) { return; }
		if (document.addEventListener) { elem.addEventListener(eventType, handler, useCapture || false);}
		else { elem.attachEvent("on" + eventType, handler); }
	};
	ElemWrapper.prototype.bind = function(eventType, handler, useCapture) {
		this.each(function(index, elem) {
			SIMPLE.bind(elem, eventType, handler, useCapture);
		});
	};
	/** unbind */
	SIMPLE.unbind = function(elem, eventType, handler, useCapture) {
		if (document.addEventListener) { elem.removeEventListener(eventType, handler, useCapture || false); }
		else { elem.detachEvent("on" + eventType, handler); }
	};
	ElemWrapper.prototype.unbind = function(eventType, handler, useCapture) {
		this.each(function(index, elem) {
			SIMPLE.unbind(elem, eventType, handler, useCapture);
		});
	};
	SIMPLE.each( ("blur focus focusin focusout load resize scroll unload click dblclick " +
			"mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave " +
			"change select submit keydown keypress keyup error").split(" "), function(index, name) {
		SIMPLE[name] = function(elem, handler, useCapture) {SIMPLE.bind(elem, name, handler, useCapture);};
		ElemWrapper.prototype[name] = function(handler, useCapture) { // ElemWrapper // $()
			this.each(function(index, elem) {
				SIMPLE.bind(elem, name, handler, useCapture);
			});
		};
	});
	/**
	 * fire event programmatically
	 * @see tangram
	 */
	SIMPLE.trigger = function (elem, eventType, options) {
		options = options || {}; // make sure options exists.
		var o = {
			bubbles : options.bubbles || true,
			cancelable : options.cancelable || true,
			view : options.view || window,
			detail : options.detail || 1,
			screenX : options.screenX || 0,
			screenY : options.screenY || 0,
			clientX : options.clientX || 0,
			clientY : options.clientY || 0,
			ctrlKey : options.ctrlKey || false,
			altKey  : options.altKey || false,
			shiftKey: options.shiftKey || false,
			metaKey : options.metaKey || false,
			keyCode : options.keyCode || 0,
			charCode: options.charCode || 0,
			button  : options.button || 0,
			relatedTarget : options.relatedTarget || null
		};
		var event, // Event obj 
			l_KeyboardEvents = " keydown keyup keypress ",
			l_MouseEvents = " click dblclick mousedown mousemove mouseup mouseover mouseout ",
			l_HTMLEvents = " abort blur change error focus load reset resize scroll select submit unload ",
			eventCategory, // event category, such as "UIEvents", "MouseEvents", "MutationEvents", and "HTMLEvents"
			initXXEventParas;
		if(document.createEvent){ //DOM
			/**
			 * var event = document.createEvent(eventCategory);
			 * event.initXXEvent(eventType, ....);
			 * elem.dispatchEvent(event);
			 */
			
			if (l_KeyboardEvents.indexOf(" " + eventType + " ") >= 0) { 
				eventCategory = "KeyEvents"; 
				initXXEventParas = [eventType, o.bubbles, o.cancelable, o.view, o.ctrlKey, o.altKey, o.shiftKey, o.metaKey, o.keyCode, o.charCode];
				event = document.createEvent(eventCategory);
				event.initKeyEvent.apply(event, initXXEventParas);
			}
			else if (l_MouseEvents.indexOf(" " + eventType + " ") >= 0) { 
				eventCategory = "MouseEvents"; 
				initXXEventParas = [eventType, o.bubbles, o.cancelable, o.view, o.detail, o.screenX, o.screenY, o.clientX, o.clientY, o.ctrlKey, o.altKey, o.shiftKey, o.metaKey, o.button, o.relatedTarget];
				event = document.createEvent(eventCategory);
				event.initMouseEvent.apply(event, initXXEventParas);
			}
			else if (l_HTMLEvents.indexOf(" " + eventType + " ") >= 0) { 
				eventCategory = "HTMLEvents"; 
				initXXEventParas = 	[eventType, o.bubbles, o.cancelable];
				event = document.createEvent(eventCategory);
				event.initEvent.apply(event, initXXEventParas);
			}
			else {
				var _msg = "SIMPLE err: trigger event `" + eventType + "' is not supported!!!";
				alert(_msg); throw _msg;
			}
			
			elem.dispatchEvent(event);
			
		}
		else { //ie, //TODO: event property is not set yet
			event = document.createEventObject();
			element.fireEvent("on" + eventType, event);
		}
	};
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	/********************************** ajax *************************************
	 createXHR ---> open ---> setRequestHeader ---> onreadystatechange ---> send
	                   
	                    readyState
	 0	UNINITIALIZED	open()has not been called yet.
	 1	LOADING	send()has not been called yet.
	 2	LOADED	send() has been called, and headers and status are available.
	 3	INTERACTIVE	Downloading; responseText holds partial data.
	 4	COMPLETED	The operation is complete.
	 *****************************************************************************/
	//adapter
	if(typeof window.XMLHttpRequest === "undefined"){
		window.XMLHttpRequest = function(){
			return new window.ActiveXObject(navigator.userAgent.indexOf("MSIE 5") >=0 ? "Microsoft.XMLHTTP" : "Msxml2.XMLHTTP");
		};
	}
	
	SIMPLE.ajaxError = function(resp, status, xhr) {
		
	};
	
	SIMPLE.checkJson = function(jsonObj) {
		return !!jsonObj;
	};
	
	/**
	 * set up an ajax
	 * 
	 * @param options - options property
	 *     url - url
	 *     method - method
	 *     async - whether or not to perform the operation asynchronously
	 *     timeout -  timeout (in milliseconds) for the xhr. ontimeout will not be called, if not given(never timeout).
	 *     dataType - the xhr response data type. xml text json
	 *     onsuccess - callback
	 *     onerror - callback
	 *     oncomplete - callback
	 *     ontimeout - callback, if timeout if given
	 */
	SIMPLE.ajax = function(url, options) {
		url = url || "";
		if ( typeof url === "object" ) { options = url; url = undefined; }
		options = options || {};
		var xhr,
			o = {
				url: url || options.url,
				method: (options.method || "GET").toUpperCase(),
				data: options.data || null, //Data to be sent to the serve
				async: options.async || true,
				timeout: options.timeout || null,
				dataType: options.dataType || "text", //response data type
				onsuccess: options.onsuccess || function(){}, // three argument: respData, status, xhr
				onerror: options.onerror || SIMPLE.ajaxError, // three argument: respData, status, xhr
				oncomplete: options.oncomplete || function(){}, // three argument: respData, status, xhr
				ontimeout: options.ontimeout || function(){},
				contentType: options.contentType || "application/x-www-form-urlencoded"
			},
			isTimeout = false,
			isComplete = false; 
		
		// convert plain obj to query str
		if (o.data && typeof o.data === "object") {//data is obj
			var a = [];
			for (name in o.data) { a.push(encodeURIComponent(name) + "=" +  encodeURIComponent(o.data[name])); }
			o.data = a.join("&");
		}
		else if (o.data && typeof o.data === "string") {o.data=encodeURIComponent(o.data);}
		
		// process get method
		if (o.method === "GET" && o.data) { 
			o.url += (o.url.indexOf("?") >=0 ? "&" : "?") + o.data;
			o.data = null;//data have been append to url; send(null)
		}
		
		
		// create xhr
		xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
		// ---> first ---> open
		xhr.open( o.method, o.url, o.async );
		
		// ---> second ---> set request header
		xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
		if (o.method === "POST") xhr.setRequestHeader("Content-Type",o.contentType);
		
		// for login verify error
		xhr.setRequestHeader("Q-Ajax-Request","true");
		
		// ---> third ---> register ready datate change handler
		xhr.onreadystatechange = function() {
			if (4 === xhr.readyState) {xhrCompleted(xhr); xhr = null;}
		};
		
		// ---> fourth ---> send the request
		xhr.send(o.data);
		
		
		
		if ( o.async && o.timeout && parseInt(o.timeout) > 0 && (xhr.readyState > 0 && xhr.readyState < 4)) {
			setTimeout( function(){
				if (isComplete) { return; }; // has complete
				isTimeout = true; xhr.abort(); o.ontimeout(); xhr = null;
			}, o.timeout );
		}
		
		
		/**
		 * XMLHttpRequest completed 4 === xhr.readyState
		 */
		function xhrCompleted(xhr) {
			if (isTimeout) return; 
			isComplete = true;
			
			var resp;
			if (o.dataType === "xml") { resp = xhr.responseXML;}
			else if(o.dataType === "json") { resp = SIMPLE.parseJSON(xhr.responseText); }
			else { resp = xhr.responseText; }
			
			if(o.dataType === "json" && !SIMPLE.checkJson(resp)) {
				console.log('jsonerr url:'+o.url);
				o.onerror(resp, xhr.status, xhr);
			}
			else if(xhr.getResponseHeader("Q-Ajax-Status")!="" && xhr.getResponseHeader("Q-Ajax-Status")!="OK"){
				
				console.log('invalid uin url:'+o.url + " header:" + xhr.getResponseHeader("Q-Ajax-Status"));
				o.onerror(resp, xhr.status, xhr);
			}
			else if ( (xhr.status >= 200 && xhr.status < 300)
					|| xhr.status === 304) {
				o.onsuccess(resp, xhr.status, xhr);
			}
			else {
				console.log('jsonerr url:'+url);
				o.onerror(resp, xhr.status, xhr);
			}
			
			o.oncomplete(resp, xhr.status, xhr);
		} // $$$ function xhrCompleted(xhr) {
		
		return xhr;
	}; // $$$ SIMPLE.ajax = function(url, options) {
	
	
	
	/*** ajax help function ***/
	/**
	 * @param url - url
	 * @param data - query data
	 * @param succCB - call back function, if success
	 * @param errCB - call back function, if error
	 * @param timeoutCB - call back function, if timeout
	 */
	SIMPLE.getJSON = function(url, data, succCB, errCB, timeoutCB) {
		SIMPLE.ajax(url, {
			data: data,
			dataType: "json",
			onsuccess: succCB,
			onerror: errCB,
			ontimeout: timeoutCB
		});
	};
	//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ ajax end $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	/**************************************************
	 * cookie util
	 **************************************************/
	SIMPLE.cookie = {
		get : function(name) {
			name = document.cookie.match(RegExp("(^| )" + name + "=([^;]*)(;|$)"));
			return !name ? "" : unescape(name[2]);
		},
		/**
		 * set cookie
		 * @param name - cookie name
		 * @param domain - cookie domain. current domain, if not given.
		 * @param path - cookie path. /, if not given.
		 * @param expires - expires. 单位是天，如果未给出，则保存一个月。
		 */
		set : function(name, value, domain, path, expires) {
			var date = new Date();
			date.setTime(date.getTime() + (expires ? 36E5 * expires : 2592E6));
			document.cookie = name + "=" + value 
			+ (domain ? "; domain=" + domain : "")
			+ "; path=" + (path ? path : "/") 
			+ "; expires=" + date.toGMTString();
					
		},
		/**
		 * delete the cookie
		 * @param name - cookie name
		 * @param domain - cookie domain. current domain, if not given.
		 * @param path - cookie path. root path(/), if not given.
		 */
		del : function(name, domain, path) {
			document.cookie = name + "=" 
					+ (domain ? "; domain=" + domain : "")
					+ "; path=" + (path ? path : "/") 
					+ "; expires=Mon, 26 Jul 1997 05:00:00 GMT";
		},
		/**
		 * delete all cookie in current domain and root path(/).
		 */
		delAll : function() {
			var n,
				arr = document.cookie.match(RegExp("([^ ;][^;]*)(?=(=[^;]*)(;|$))",
					"gi"));
			for (n in arr)
				document.cookie = arr[n]
						+ "=; path=/; expires=Mon, 26 Jul 1997 05:00:00 GMT";
		}
	}; // $$$ SIMPLE.cookie 
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	/********************************** template engine *************************************
	tmpl(string/id, [obj]);
		1. 第一个参数(必选)
		id: 通过getElementById(str).innerHTML获取字符串. tmpl会被调用两次
		字符串: ajax拉取等
		2. 第二个参数(可选)
		obj对象
		如未给出，会返回获取html代码的方法
		如果给出，返回html代码
		demo
		tmpl(id, obj);
		tmpl(tmpl-string, obj)
		tmpl-fn = tmpl(id); tmpl-fn(obj);  //先获取script fun, 然后在传入obj，执行。如果需要多次执行,可自己缓存tmpl-fn.
		<% print("hi") %>
		<%= expression %> 和 <% codeblock %>
	 ****************************************************************************************/
	var _TMPL_cache = {}; 
	SIMPLE.tmpl = function tmpl(str, data) {
	try {
		// Figure out if we're getting a template, or if we need to
		// load the template - and be sure to cache the result.
		//str参数中不包含<时，认为str参数为id时，获取该id包含template string,缓存。
		var fn =  (-1 == str.indexOf("<")) ?
				_TMPL_cache[str] =  _TMPL_cache[str] || tmpl(document.getElementById(str).innerHTML) :
		
		// Generate a reusable function that will serve as a template
		// generator (and which will be cached).
		new Function("obj",
		
		// ^^^ begin of function body
		"var p=[],print=function(){p.push.apply(p,arguments);};" 

			// Introduce the data as local variables using with(){}
			+ "with(obj){p.push('" +
	
			// Convert the template into pure JavaScript
			// --->              replace
			// 1句将\r\t\n变成空格 
			// 2,3,4句主要是将标签外的'进行转义，改成\'。不过此时不能改变标签内的'
			// 5句处理<%= %>标签
			str.replace(/[\r\t\n]/g, " ")          // 1. \r\t\n ---> space
			   .replace(/'(?=[^#]*#>)/g,"\t")      // 2. <% %><%= %>标签中的' ---> \t
			   .split("'").join("\\'")             // 3. 其他地方的' ---> \'
			   .split("\t").join("'")              // 4. \t ---> ' 还原标签<% %><%= %>中的'
			   .replace(/<#=(.+?)#>/g, "',$1,'")   // 5. 处理<%= expr %> ---> ', expr ,'
			   .split("<#").join("');")            // 6. <% ---> ');
			   .split("#>").join("p.push('")       // 7. %> ---> p.push('
			   + "');}"  // end of with
			   + "return p.join('');"); // $$$ end of function body
	        
		// Provide some basic currying to the user
        return data ? fn(data) : fn;
	} // $$$ try
	catch(err) {
		console.log("SIMPLE.tmpl err: " + err);
	}
	}; // $$$ SIMPLE.tmpl = function tmpl(str, data) {
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	ElemWrapper.prototype.ready = SIMPLE.ready;
	
	
	
	
	
	/********* expose SIMPLE to the global object  **********/
	window.SIMPLE = window.$ = SIMPLE;
	
})(window); // $$$ (function( window, undefined ) {

