/*****************************************************************************
 * 文件名:    cbc-1.0.js
 * 项目:      cbc
 * 描述:      提供简易的 DOM 操作与常用工具函数，简化 Web 开发中的交互操作。
 * 作者:      Lion
 * 创建日期:  2024年9月6日
 * 更新历史:  
 *   - 版本 1.0.0: 初始版本，提供 DOM 选择、属性操作、3D 翻转、表单复选框控制等功能。
 * 
 * 版权声明:  
 *   版权所有 © 2024 3578.cn，保留所有权利。
 *   此代码仅供内部使用，未经许可禁止分发或商用。
 *****************************************************************************/

// 使用说明：
// 1. 初始化选择器：使用 $ 或 cbc(selector) 来选中元素，可以传入 id 字符串或 DOM 对象。
//    例如：var elem = $("myElementId");
// 2. 获取或设置元素的属性：可以通过 .value, .innerHTML, .style, .className 来获取或设置元素的值。
//    例如：elem.value = "新值"; 或 var value = elem.value;
// 3. 扩展功能：
//    - .ready(fn)：当 DOM 加载完成时执行回调函数。
//      例如：$(document).ready(function () { console.log("DOM 已加载"); });
//    - .rotate(way, degree, scale, isContinue)：为元素添加 3D 翻转效果。
//      例如：elem.rotate("X", 0, 5, true);
//    - .insertAfter(newElement)：在当前元素后插入新的 DOM 元素。
//      例如：elem.insertAfter(document.createElement("div"));
//    - .querySelector(selector), .querySelectorAll(selector)：查询子元素。
//      例如：var child = elem.querySelector(".child");
// 4. 全局功能：
//    - $.write(str)：向页面输出字符串。
//      例如：$.write("Hello World!");
//    - $.format(str, ...args)：格式化字符串。
//      例如：$.format("Hello, {0}", "World"); // 输出：Hello, World
//    - $.getQuery(name)：获取 URL 查询参数。
//      例如：var param = $.getQuery("id");
//    - $.dateFormat(date, formatString)：格式化日期。
//      例如：var formattedDate = $.dateFormat(new Date(), "yyyy-MM-dd");
// 5. 自定义扩展：
//    - Array.prototype.remove(n)：从数组中移除指定索引的元素。
//      例如：var arr = [1, 2, 3]; arr = arr.remove(1); // arr 变为 [1, 3]
//    - NodeList.prototype.forEach(callback, thisArg)：遍历 NodeList 对象。
//      例如：document.querySelectorAll("div").forEach(function (div) { div.style.color = "red"; });
// 6. 脚本加载：使用 LoadJs(jsUrl) 动态加载 JS 脚本。
//    例如：LoadJs("https://example.com/script.js");
// 7. 表单复选框控制：
//    - getSelectedCheckboxValues(valueName)：返回选中的复选框值。
//      例如：var selectedValues = getSelectedCheckboxValues("checkboxName");
//    - toggleAllCheckboxes(checkId, valueName)：全选或取消全选。
//      例如：toggleAllCheckboxes("checkAllId", "checkboxName");
//    - invertAllCheckboxes(checkId, valueName)：反选。
//      例如：invertAllCheckboxes("checkAllId", "checkboxName");
//    - areAllCheckboxesSelected(valueName)：判断是否全部复选框已选中。
//      例如：var allSelected = areAllCheckboxesSelected("checkboxName");
//    - isAnyCheckboxSelected(valueName)：判断是否有复选框被选中。
//      例如：var anySelected = isAnyCheckboxSelected("checkboxName");

/**
 * 释放 jQuery 对 $ 符号的控制。
 */
$.noConflict();

/**
 * cbc 控制器的构造函数。
 * @param {string|HTMLElement} selector - 可以是元素的 ID 或 DOM 对象。
 * @returns {cbc} 返回 cbc 实例对象。
 */
window.cbc = window.$ = (function () {
    var cbc = function (selector) {
        return new cbc.fn.init(selector); // 初始化选择器。
    };

    // 定义对象属性和方法。
    cbc.fn = cbc.prototype = {
        version: "1.0",

        /**
         * 获取元素的 value 属性。
         * @returns {string|undefined} 返回元素的 value 属性或 undefined。
         */
        get value() {
            return this[0] ? this[0].value : undefined;
        },
        /**
         * 设置元素的 value 属性。
         * @param {string} val - 要设置的 value 值。
         */
        set value(val) {
            if (this[0]) this[0].value = val;
        },

        /**
         * 获取元素的 innerHTML 属性。
         * @returns {string|undefined} 返回元素的 innerHTML 属性或 undefined。
         */
        get innerHTML() {
            return this[0] ? this[0].innerHTML : undefined;
        },
        /**
         * 设置元素的 innerHTML 属性。
         * @param {string} val - 要设置的 innerHTML 值。
         */
        set innerHTML(val) {
            if (this[0]) this[0].innerHTML = val;
        },

        /**
         * 获取元素的 style 属性。
         * @returns {CSSStyleDeclaration|undefined} 返回元素的 style 属性或 undefined。
         */
        get style() {
            return this[0] ? this[0].style : undefined;
        },
        /**
         * 设置元素的 style 属性。
         * @param {CSSStyleDeclaration} val - 要设置的 style 值。
         */
        set style(val) {
            if (this[0]) this[0].style = val;
        },

        /**
         * 获取元素的 className 属性。
         * @returns {string|undefined} 返回元素的 className 属性或 undefined。
         */
        get className() {
            return this[0] ? this[0].className : undefined;
        },
        /**
         * 设置元素的 className 属性。
         * @param {string} val - 要设置的 className 值。
         */
        set className(val) {
            if (this[0]) this[0].className = val;
        },

        /**
         * 获取元素的 type 属性。
         * @returns {string|undefined} 返回元素的 type 属性或 undefined。
         */
        get type() {
            return this[0] ? this[0].type : undefined;
        }
    };

    // 扩展方法。
    cbc.extend = cbc.fn.extend = function () {
        var target = this, options;
        for (var i = 0; i < arguments.length; i++) {
            options = arguments[i];
            for (var name in options) {
                target[name] = options[name]; // 复制属性。
            }
        }
        return target;
    };

    /**
     * 初始化选择器。
     * @param {string|HTMLElement} selector - 选择器可以是元素的 ID 或 DOM 对象。
     * @returns {cbc} 返回当前实例。
     */
    var init = cbc.fn.init = function (selector) {
        if (!selector) return this;

        if (typeof selector === "string") {
            var elem = document.getElementById(selector);
            if (elem) {
                this[0] = elem;
                this.length = 1;
            }
        } else if (typeof selector === "object") {
            this[0] = selector;
            this.length = 1;
        }
        return this;
    };

    init.prototype = cbc.fn;

    // 扩展功能。
    cbc.fn.extend({
        /**
         * 当 DOM 加载完成后执行回调函数。
         * @param {function} fn - DOM 加载完成后的回调函数。
         */
        ready: function (fn) {
            document.addEventListener("DOMContentLoaded", fn);
        },

        /**
         * 为元素添加 3D 翻转效果。
         * @param {string} way - 翻转方向 ("X" 或 "Y")。
         * @param {number} degree - 起始度数。
         * @param {number} scale - 翻转尺度。
         * @param {boolean} isContinue - 是否持续翻转。
         */
        rotate: function (way, degree, scale, isContinue) {
            var obj = this[0], i = 0;
            cbc.intervalLoop(function () {
                obj.style.transform = "rotate" + way + "(" + (degree + i * scale) + "deg)";
                i = (i + 1) % (360 / scale);
            }, scale, isContinue ? 0 : 180 / scale + 1);
        },

        /**
         * 在当前元素后插入新元素。
         * @param {HTMLElement} newElement - 要插入的新 DOM 元素。
         */
        insertAfter: function (newElement) {
            var parent = this[0].parentNode;
            parent.insertBefore(newElement, this[0].nextSibling); // 插入元素。
        },

        /**
         * 根据 CSS 选择器查询子元素。
         * @param {string} selector - CSS 选择器。
         * @returns {HTMLElement} 返回匹配的子元素。
         */
        querySelector: function (selector) {
            return this[0].querySelector(selector);
        },

        /**
         * 根据 CSS 选择器查询所有匹配的子元素。
         * @param {string} selector - CSS 选择器。
         * @returns {NodeList} 返回匹配的所有子元素。
         */
        querySelectorAll: function (selector) {
            return this[0].querySelectorAll(selector);
        }
    });

    // 全局控制方法。

    /**
     * 向页面输出字符串。
     * @param {string} str - 要输出的字符串。
     */
    cbc.write = function (str) {
        document.write(str);
    };

    /**
     * 循环执行函数。
     * @param {function} fn - 要执行的函数。
     * @param {number} time - 间隔时间（毫秒）。
     * @param {number} count - 循环次数，0 表示无限循环。
     */
    cbc.intervalLoop = function (fn, time, count) {
        var i = 0;
        var loop = function () {
            if (count === 0 || i < count) {
                setTimeout(loop, time);
                fn();
                i++;
            }
        };
        loop();
    };

    /**
     * 格式化字符串，将占位符 {0}, {1}, ... 替换为相应参数。
     * @param {string} str - 要格式化的字符串。
     * @param {...*} args - 替换占位符的参数。
     * @returns {string} 返回格式化后的字符串。
     */
    cbc.format = function (str) {
        for (var i = 1; i < arguments.length; i++) {
            str = str.replace("{" + (i - 1) + "}", arguments[i] !== undefined ? arguments[i] : "");
        }
        return str;
    };

    /**
     * 获取 URL 查询参数。
     * @param {string} name - 要获取的查询参数的名称。
     * @returns {string|null} 返回查询参数的值或 null。
     */
    cbc.getQuery = function (name) {
        var regex = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var result = window.location.search.substr(1).match(regex);
        return result ? decodeURIComponent(result[2]) : null;
    };

    /**
     * 格式化日期。
     * @param {string|Date} date - 日期字符串或 Date 对象。
     * @param {string} formatString - 日期格式字符串，如 "yyyy-MM-dd"。
     * @returns {string} 返回格式化后的日期字符串。
     */
    cbc.dateFormat = function (date, formatString) {
        var d = new Date(date);
        var o = {
            "yyyy": d.getFullYear(),
            "MM": ('0' + (d.getMonth() + 1)).slice(-2),
            "dd": ('0' + d.getDate()).slice(-2),
            "hh": ('0' + d.getHours()).slice(-2),
            "mm": ('0' + d.getMinutes()).slice(-2),
            "ss": ('0' + d.getSeconds()).slice(-2),
            "fff": ('00' + d.getMilliseconds()).slice(-3)
        };
        for (var k in o) {
            formatString = formatString.replace(k, o[k]);
        }
        return formatString;
    };

    return cbc;
})();

/**
 * 获取选中的复选框的值。
 * @param {string} valueName - 复选框的 name 属性值。
 * @returns {string} 返回选中的复选框的 value 值集合，以逗号分隔。
 */
function getSelectedCheckboxValues(valueName) {
    var values = [];
    document.querySelectorAll('input[name="' + valueName + '"]:checked').forEach(function (checkbox) {
        values.push(checkbox.value);
    });
    return values.join(",");
}

/**
 * 全选或取消全选复选框。
 * @param {string} checkId - 控制全选的复选框 ID。
 * @param {string} valueName - 复选框的 name 属性值。
 */
function toggleAllCheckboxes(checkId, valueName) {
    var allChecked = areAllCheckboxesSelected(valueName);
    document.querySelectorAll('input[name="' + valueName + '"]').forEach(function (checkbox) {
        checkbox.checked = !allChecked;
    });
    document.getElementById(checkId).checked = !allChecked;
}

/**
 * 反选复选框。
 * @param {string} checkId - 控制全选的复选框 ID。
 * @param {string} valueName - 复选框的 name 属性值。
 */
function invertAllCheckboxes(checkId, valueName) {
    document.querySelectorAll('input[name="' + valueName + '"]').forEach(function (checkbox) {
        checkbox.checked = !checkbox.checked;
    });
    document.getElementById(checkId).checked = areAllCheckboxesSelected(valueName);
}

/**
 * 判断是否所有复选框都被选中。
 * @param {string} valueName - 复选框的 name 属性值。
 * @returns {boolean} 如果所有复选框都被选中，返回 true，否则返回 false。
 */
function areAllCheckboxesSelected(valueName) {
    return Array.from(document.querySelectorAll('input[name="' + valueName + '"]')).every(function (checkbox) {
        return checkbox.checked;
    });
}

/**
 * 判断是否有复选框被选中。
 * @param {string} valueName - 复选框的 name 属性值。
 * @returns {boolean} 如果有复选框被选中，返回 true，否则返回 false。
 */
function isAnyCheckboxSelected(valueName) {
    return Array.from(document.querySelectorAll('input[name="' + valueName + '"]')).some(function (checkbox) {
        return checkbox.checked;
    });
}