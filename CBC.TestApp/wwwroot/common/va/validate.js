/**************************************************************
* 公共表单输入验证方法。
**************************************************************/
/******************
错误消息控制类。
******************/
if (!cb) {
    var cb = {};
}
if (!cb.va) {
    cb.va = {};
}
if (!cb.va.ui) {
    cb.va.ui = {};
}

//va.ui
(function() {
    //abstract: 错误类别基类。
    function errbase() { }
    errbase.prototype.errmsg = "";

    //---------------------------------------
    //public : 附加错误消息。
    function createerr() { }
    createerr.prototype = new errbase();
    //是否已经显示错误消息。
    createerr.prototype.haserr = function(Obj) {

        if (Obj.nextSibling.nextSibling && Obj.nextSibling.nextSibling.className == "cberr") {
            return true;
        }
        else {
            return false;
        }
    };

    //清除错误。
    createerr.prototype.clearerr = function() {

    };

    //@param {object} obj
    createerr.prototype.show = function(obj) {
        if (!(obj && obj.nodeType == 1)) {
            return;
        }

        //在当前节点后插入新节点。
        //@param {object} n 被插入的节点。
        //@r {object} r 当前节点。
        var insertAfter = function(n, r) {
            if (r.nextSibling)
                r.parentNode.insertBefore(n, r.nextSibling);
            else
                r.parentNode.appendChild(n);
        };

        //附加错误消息。
        //处理复选框。
        var Obj = obj;
        var cnode;
        if ((Obj.type && Obj.type == "checkbox") || (Obj.type && Obj.type == "radio")) {
            if (Obj.name && document.getElementsByName(Obj.name).length > 0) {
                Obj = document.getElementsByName(Obj.name)[document.getElementsByName(Obj.name).length - 1];

                if (Obj.nextSibling.nodeType == 3) {
                    cnode = Obj.nextSibling;
                }
                else {
                    cnode = Obj;
                }
            }
        }
        else {
            cnode = Obj;
        }

        var newlabel = document.createElement("label");
        newlabel.innerHTML = this.errmsg;
        insertAfter(newlabel, cnode);
    };

    //public:将错误消息显示在指定的容器内。
    cb.va.ui.createerr = createerr;

    //-------------------------------
    //在指定容器中显示错误消息。
    //@param {string} id 错误容器的id。
    function showmsg() {
        this.errid = "";
    }
    showmsg.prototype = new errbase();
    //是否已经显示错误。
    showmsg.prototype.haserr = function() {
        if (this.errid != "" && document.getElementById(this.errid).innerHTML != "") {
            return true;
        }
    };

    //清除错误。
    showmsg.prototype.clearerr = function() {

    };

    showmsg.prototype.show = function(id) {
        if (!(typeof id == "string") && !(typeof this.errmsg == "string")) {
            return;
        }
        try {
            this.errid = id;
            document.getElementById(id).innerHTML = this.errmsg;
        }
        catch (e) {
            alert("未找到对象。");
        }
    };

    //public
    cb.va.ui.showmsg = showmsg;

    //-------------------------------------
    //private: 弹出错误消息框。
    //@param {string} errmsg 错误消息。
    function alert_err() { }
    alert_err.prototype = new errbase();
    alert_err.prototype.show = function(obj) {
        if (!(typeof this.errmsg == "string")) {
            return;
        }
        alert(this.errmsg);

        obj.focus();

        throw new Object(); //抛出异常终止程序执行。
    };

    //public
    cb.va.ui.alert_err = alert_err;

    //错误工厂类。
    cb.va.ui.errfac = function(mode, msg, obj) {

        switch (mode) {
            case "alert":
                //alert
                var alerterr = new alert_err();
                alerterr.errmsg = msg;
                alerterr.show(obj);
                //end
                break;

            case "append":
                //附加错误消息。
                var temp1 = new createerr();
                temp1.errmsg = msg;

                //@param {object} 需要附加的对象。
                temp1.show(obj);
                //end
                break;

            case "errcontent":
                //指定容器。
                var errcontent = new showmsg();
                errcontent.errmsg = msg;
                //@param {string} 容器的id。
                errcontent.show(obj);
                break;
        }
    };
})();

/******************
验证库核心类。
******************/

//va.validate
(function() {

    //验证规则对象。
    cb.va.rules = {};

    //验证对象。
    //public
    //vamanager
    function vamanager() {
        this.all = {}; //存储所有验证实例。
        this.errs = {}; //错误信息。
        this.errmode = "alert"; //错误消息显示模式。
        this.isdone = true; //是否通过验证。
    }

    //设置错误消息显示模式。
    //@param {string} mode 模式名称alert、append、errcontent。
    vamanager.prototype.seterrMode = function(mode) {
        this.errmode = mode;
    };

    //返回当前输入框是否验证通过。
    vamanager.prototype.getisok = function() {
        return this.isdone;
    };

    //获取验证实例。
    //@param {string} id 需要验证输入框的id。
    //@param {boolean} isrequired 是否是必填项。
    //@param {Object/string} errobj 显示错误消息的容器。
    vamanager.prototype.getValidate = function(id, errobj) {
        //GLog.write("要验证的输入框是:" + id);
        this.all[id] = new validate(id, errobj, this.errmode);
        return this.all[id];
    };

    //将验证对象，添加到验证列表。
    vamanager.prototype.add = function(obj) {
        if (!(obj instanceof validate)) {
            alert("传入参数错误。");
            return;
        }

        this.all[obj.getid()] = obj; //添加验证对象。
    };

    //提交时验证。
    vamanager.prototype.validate = function(fn) {
        try {
            for (var x in this.all) {
                this.all[x].exec();
                this.errs[x] = this.all[x].isruned();
                //GLog.write("验证" + x);
            }

            //alert方式时,通过验证。
            if (fn && !(typeof fn == "function")) {
                alert("请传入函数");
                this.isdone = false;
            }
            else {
                if (this.errmode != "alert") {
                    var ispass = true;
                    for (var y in this.errs) {
                        if (!this.errs[y]) {
                            ispass = false;
                            break;
                        }
                    }

                    if (!ispass) {
                        //GLog.write(i + "未通过验证");
                        this.isdone = false;
                    }
                    else {
                        this.isdone = true;
                        fn();
                    }
                }
                else {
                    this.isdone = true;
                    fn();
                }
            }
        }
        catch (e) {
            //alert方式时，捕获错误以终止程序执行。
            //GLog.write("alert验证未通过，终止执行！");
            this.isdone = false;
        }
    };

    //开放验证类。
    cb.va.vamanager = vamanager;

    //验证单个输入框。
    //private
    function validate(id, errobj, errmode) {
        if (id == undefined && errobj == undefined && errmode == undefined) {
            //alert("请检查向validate构造器传入的参数！");
        }
        this.id = id; //验证框id。
        this.element = document.getElementById(id);
        this.errMode = errmode; //错误消息显示模式。
        this.param;
        this.rules = {}; //验证规则列表。
        this.errobj = errobj; //错误消息显示容器。
        this.iserr = true; //是否通过验证状态。
        this.isexec = false; //是否执行过exec方法。
        this.startfn; //开始验证前的函数。
        this.endfn; //验证结束后执行。
        this.errmsg = ""; //错误消息。
    }

    //设置开始执行回调。
    //@param {function} fn 验证结束后执行。
    validate.prototype.setstart = function(fn) {
        this.startfn = fn;
    };

    //设置结束执行回调。
    //@param {function} fn 验证结束后执行。
    validate.prototype.setend = function(fn) {
        this.endfn = fn;
    };

    //添加具体的验证规则。
    //@param {string} key 对象唯一索引。
    //@param {object} obj 验证规则对象。
    //@errmsg{string} errmsg 错误提示。
    validate.prototype.add = function(obj, errmsg) {
        ////GLog.write("添加具体验证规则:" + this.id);
        if (obj == undefined && errmsg == undefined) {
            alert("请检查add方法，传入的参数！");
        }
        var key = "" + Math.random();
        this.rules[key] = [obj, errmsg];
    };

    //获取验证对象id。
    validate.prototype.getid = function() {

        if (this.id != undefined) {
            return this.id;
        }
    };

    //附加参数。
    //@param {string} param 额外参数，将传入验证规则对象
    validate.prototype.setparam = function(param) {
        //GLog.write("添加参数");
        this.param = param;
    };

    //执行exec后，判断是否通过验证。
    validate.prototype.isruned = function() {

        if (!this.isexec) {
            //alert("尚未执行验证过程");
            return -1;
        }
        return this.iserr;

    };

    //是否执行过验证。
    validate.prototype.isvalidated = function() {
        return this.isexec;
    };

    //获取错误消息。
    validate.prototype.geterrmsg = function() {

        return this.errmsg;
    };

    //执行验证。
    validate.prototype.exec = function() {
        try {
            //alert弹出错误消息时，捕获alert抛出的异常。
            //GLog.write("验证前");
            //GLog.write("执行验证");
            if (this.startfn) {
                this.startfn();
            }
            for (var x in this.rules) {
                //GLog.write("验证关键字：" + x);
                if (this.rules[x][0] == cb.va.rules.required) {
                    result = this.rules[x][0](this.element.value, this.element, this.param);
                }
                else {
                    if (this.element.value != '') {
                        result = this.rules[x][0](this.element.value, this.element, this.param);
                    }
                    else {
                        result = true;
                    }
                }
                //GLog.write("验证结果：" + result);
                if (result) {
                    this.iserr = true;
                    continue;
                }
                else {
                    var errmsg = "";
                    this.errmsg = this.rules[x][1];
                    errmsg = this.rules[x][1];

                    //调用错误消息显示类。
                    if (this.errMode == "errcontent") {

                        cb.va.ui.errfac(this.errMode, errmsg, this.errobj);
                    } else {
                        //GLog.write("错误:" + errmsg);
                        cb.va.ui.errfac(this.errMode, errmsg, this.element);
                    }
                    this.iserr = false;
                    //GLog.write("显示错误提示:" + this.rules[x][1]);
                    break;
                }
            }

            //执行回调。
            if (this.iserr) {
                //GLog.write("exec通过验证！");

                if (arguments[0] != undefined) {
                    //GLog.write("执行成功");
                    arguments[0]();
                }
            }
            else {
                //GLog.write("exec未通过验证！");
                if (arguments[1] != undefined) {
                    //GLog.write("失败1");
                    arguments[1]();
                }
            }
            this.isexec = true;
            //GLog.write("验证后");

            if (this.endfn) {
                this.endfn();
            }

        }
        catch (e) {
            this.iserr = false;

            //GLog.write("exec未通过验证！");
            if (arguments[1] != undefined) {
                //GLog.write("执行失败2");
                arguments[1]();
            }
            this.isexec = true;
            //GLog.write("验证后");
            if (this.endfn) {
                this.endfn();
            }
            throw e;
        }
    };

    //获取单个验证实例。
    cb.va.get = function(id, errobj, errmode) {
        ///<summary>获取单个验证对象</summary>
        ///<param name="id">输入框的id</param>
        ///<param name="errobj">指定错误框的id</param>
        ///<param name="errmode">错误显示模式：alert、append、errcontent</param>
        if (id == undefined) {
            alert("必须填写控件id!");
            return false;
        }
        var mode = errmode || "alert";
        var obj = errobj || "";
        return new validate(id, obj, mode);
    };

    //开放验证对象。
    cb.va.validate = validate;
})();

/*
配置解释类。
*/
(function() {
    //配置类。
    //@param {object} vali 验证对象。
    function config(vali, option) {
        this.all = {}; //存储验证实例。
        this.va = vali;
        this.vaoption = option;
    }

    //设置实例。
    //@param {string} id 控件的id。
    config.prototype.setinstance = function(id, errmode) {
        //@param {string} id。
        //@param {string} 错误容器。
        this.all[id] = this.va.getValidate(id, errmode);
    };

    //返回是否验证通过。
    config.prototype.isvalidated = function() {
        return this.va.getisok();
    };

    //读取实例。
    config.prototype.getinstance = function(id) {

        return this.all[id];
    };

    //初始化验证实例。
    config.prototype.init = function(fn) {
        //GLog.write("开始读取配置");
        for (var x in this.vaoption) {
            var temp;
            if (x == "errcontent") {
                continue;
            }
            if (x == "errmode") {
                continue;
            }
            else {
                var errmode;
                if (this.vaoption["errmode"] == "errcontent") {
                    errmode = this.vaoption[x]["errcontent"];
                }
                //GLog.write("调用方法，获取实例:" + x);
                //GLog.write("获取到:" + x +"实例");

                this.setinstance(x, errmode);
                temp = this.getinstance(x);
            }

            for (var y in this.vaoption[x]) {
                if (y == "param") {
                    //GLog.write("添加附加参数:" + this.vaoption[x][y]);
                    //alert(this.vaoption[x][y]);
                    temp.setparam(this.vaoption[x][y]);
                    //GLog.write("设置参数结束");
                }

                if (y == "errcontent") {
                    continue;
                }
                if (y != "param") {
                    //GLog.write("调用add方法，并注册错误消息:" +  this.vaoption[x][y]);
                    temp.add(cb.va.rules[y], this.vaoption[x][y]);
                    //GLog.write("调用add" + x + "的实例:" + y);
                }
            }
        }

        //GLog.write("配置结束");
        this.va.validate(fn);
    };

    //初始化验证库。
    //@param {object} option 验证配置信息。
    //@param {function} fn 回调函数。
    cb.va.init = function(option, fn1) {
        var fn = fn1 || function() { };
        //GLog.write("创建开始");
        var vaobj = new cb.va.vamanager();
        var set = { "alert": 1, "append": 1, "errcontent": 1 };
        if ((option.errmode || "alert") in set) {
            vaobj.seterrMode(option.errmode || "alert");
        }
        else {
            throw "配置错误:errmode属性只能为alert、append、errcontent中的一种";
        }

        var temp = new config(vaobj, option);
        //GLog.write("创建结束");
        temp.init(fn);

        return temp.isvalidated();
    };
})();