"use strict";

// 1. Khởi tạo kết nối tới Hub
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// Disable nút Gửi cho đến khi kết nối thành công
document.getElementById("sendButton").disabled = true;

// 2. Lắng nghe sự kiện "ReceiveMessage" từ Server trả về
connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");

    // Style cơ bản cho các bong bóng tin nhắn
    li.style.marginBottom = "10px";
    li.style.padding = "10px 15px";
    li.style.borderRadius = "8px";
    li.style.backgroundColor = "#ffffff";
    li.style.boxShadow = "0 1px 3px rgba(0,0,0,0.12)";
    li.style.wordBreak = "break-word";

    // Tên người gửi in đậm
    var b = document.createElement("b");
    b.textContent = user + ": ";
    b.style.color = "#1976d2"; // Xanh Materialize

    // Nội dung tin nhắn
    var span = document.createElement("span");
    span.textContent = message;

    li.appendChild(b);
    li.appendChild(span);
    document.getElementById("messagesList").appendChild(li);

    // Tự động cuộn khung chat xuống cuối cùng khi có tin nhắn mới
    var chatBox = document.getElementById("chat-box");
    chatBox.scrollTop = chatBox.scrollHeight;
});

// 3. Khởi động kết nối
connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// 4. Bắt sự kiện click nút Gửi
document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;

    if (message.trim() === "") return; // Tránh gửi tin nhắn rỗng

    // Gọi hàm "SendMessage" trên Server
    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });

    // Xóa ô input sau khi gửi
    document.getElementById("messageInput").value = "";
    event.preventDefault();
});

// Bắt sự kiện nhấn phím Enter ở ô input để gửi
document.getElementById("messageInput").addEventListener("keypress", function (e) {
    if (e.key === 'Enter') {
        document.getElementById("sendButton").click();
    }
});