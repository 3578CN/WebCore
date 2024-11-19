/*****************************************************************************
 * 文件名:    ajax.js
 * 项目:      CBC
 * 描述:      通用的 Ajax 操作封装函数，简化与服务器端的交互操作。
 * 作者:      Lion
 * 创建日期:  2024年9月6日
 * 更新历史:  
 *   - 版本 1.0.0: 初始版本，包含基本的 GET 和 POST 方法。
 * 
 * 版权声明:  
 *   版权所有 © 2024 3578.cn，保留所有权利。
 *   此代码仅供内部使用，未经许可禁止分发或商用。
 *****************************************************************************/

/*
 * 公共库全局命名空间。
 * @class cbc
 * @static
 */

var cbc = cbc || {}; // 如果 cbc 不存在，则初始化 cbc 对象。
cbc.ajax = cbc.ajax || {}; // 如果 cbc.ajax 不存在，则初始化 cbc.ajax 对象。

// 自执行函数，防止全局变量污染。
(function () {
    /*
     * @module _private
     * @title  fast ajax
     */
    var _private = {
        _objPool: [], // 对象池，用于存储 XMLHttpRequest 对象。

        /*
         * 获取可用的 XMLHttpRequest 实例。
         * @method _getInstance
         * @private
         */
        _getInstance: function () {
            for (var i = 0; i < this._objPool.length; i++) {
                if (this._objPool[i].readyState === 0 || this._objPool[i].readyState === 4) {
                    return this._objPool[i]; // 返回空闲的 XMLHttpRequest 实例。
                }
            }
            // 创建新的 XMLHttpRequest 对象并放入对象池。
            var xhr = this._createXMLHttpRequest();
            this._objPool.push(xhr);
            return xhr;
        },

        /*
         * 创建新的 XMLHttpRequest 对象。
         * @method _createXMLHttpRequest
         * @private
         */
        _createXMLHttpRequest: function () {
            var xhr = null;
            if (window.XMLHttpRequest) {
                xhr = new XMLHttpRequest(); // 标准浏览器下使用 XMLHttpRequest。
            } else {
                // 兼容 IE 的 ActiveXObject 对象。
                var versions = ['Msxml2.XMLHTTP.6.0', 'MSXML2.XMLHTTP.5.0', 'MSXML2.XMLHTTP.4.0', 'MSXML2.XMLHTTP.3.0', 'Microsoft.XMLHTTP'];
                for (var i = 0; i < versions.length; i++) {
                    try {
                        xhr = new ActiveXObject(versions[i]);
                        break;
                    } catch (e) {
                        continue; // 尝试下一个版本。
                    }
                }
            }

            // 兼容某些版本的浏览器没有 readyState 属性的问题。
            if (!xhr.readyState) {
                xhr.readyState = 0;
                xhr.addEventListener("load", function () {
                    xhr.readyState = 4;
                    if (typeof xhr.onreadystatechange === "function") {
                        xhr.onreadystatechange(); // 手动触发 onreadystatechange 事件。
                    }
                });
            }
            return xhr;
        },

        /*
         * 转换数据格式，将对象或字符串转换为键值对格式。
         * @method _param
         * @param {String|Object} data 发送到服务器的数据。
         * @private
         */
        _param: function (data) {
            if (!data) return null;
            var params = [];
            if (typeof data === "object") {
                for (var key in data) {
                    if (data.hasOwnProperty(key)) {
                        params.push(encodeURIComponent(key) + "=" + encodeURIComponent(data[key]));
                    }
                }
            } else {
                data.split("&").forEach(function (item) {
                    var parts = item.split("=");
                    params.push(encodeURIComponent(parts[0]) + "=" + encodeURIComponent(parts[1]));
                });
            }
            return params.join("&").replace(/%20/g, "+");
        },

        /*
         * 发送 Ajax 请求。
         * @method _send
         * @param {String} method 请求方法，如 GET 或 POST。
         * @param {String} url 请求的 URL 地址。
         * @param {String|Object} data 发送的数据。
         * @param {Function} callback 请求成功时的回调函数。
         * @param {Function} error 请求失败时的回调函数。
         * @private
         */
        _send: function (method, url, data, callback, error) {
            var xhr = this._getInstance();
            try {
                xhr.open(method, url, true);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=UTF-8');
                xhr.send(this._param(data));

                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4) {
                        if (xhr.status === 200 || xhr.status === 304) {
                            callback && callback(xhr); // 请求成功，执行回调。
                        } else {
                            error && error(xhr); // 请求失败，执行错误回调。
                        }
                    }
                };
            } catch (e) {
                console.error(e);
            }
        },

        /*
         * 发送 POST 请求。
         * @method post
         * @param {String} url 请求的 URL 地址。
         * @param {Object} data 发送的数据。
         * @param {Function} callback 请求成功时的回调函数。
         * @param {Function} error 请求失败时的回调函数。
         * @private
         */
        post: function (url, data, callback, error) {
            this._send("POST", url, data, callback, error);
        },

        /*
         * 发送 GET 请求。
         * @method get
         * @param {String} url 请求的 URL 地址。
         * @param {Function} callback 请求成功时的回调函数。
         * @param {Function} error 请求失败时的回调函数。
         * @private
         */
        get: function (url, callback, error) {
            if (/^https?:\/\//.test(url)) {
                // 跨域请求处理。
                var script = document.createElement("script");
                script.src = url;
                script.onload = script.onreadystatechange = function () {
                    if (!script.readyState || script.readyState === "loaded" || script.readyState === "complete") {
                        callback && callback();
                        script.remove();
                    }
                };
                document.head.appendChild(script);
            } else {
                // 添加随机数防止缓存。
                url += (url.indexOf("?") > -1 ? "&" : "?") + "randnum=" + Math.random();
                this._send("GET", url, null, callback, error);
            }
        }
    };

    // 将 post 和 get 方法暴露给 cbc.ajax。
    cbc.ajax.post = _private.post;
    cbc.ajax.get = _private.get;
})();