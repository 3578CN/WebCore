"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// 在连接建立之前禁用发送按钮。
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // 这里使用 textContent 是安全的，因为不会被当作 HTML 解析，避免 XSS 风险。
    li.textContent = `${user} 说： ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;

    // 确保用户名和消息不为空
    if (user && message) {
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });
        // 清空消息输入框
        document.getElementById("messageInput").value = '';
    }

    // 防止表单的默认提交行为
    event.preventDefault();
});
