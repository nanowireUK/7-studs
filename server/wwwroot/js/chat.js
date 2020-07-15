"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var sendButton = document.getElementById("sendButton");
//Disable send button until connection is established
sendButton.disabled = true;

// Original demo function

function appendToMessagesList(el) {
    var li = document.createElement("li");
    li.append(el);
    document.getElementById("messagesList").appendChild(li);
}

function logError (err) {
    appendToMessagesList('Error: ' + err);
    return console.error(err.toString());
}

function getGameId() {
    return document.getElementById("gameId").value;
}

function getMessage() {
    return document.getElementById("messageInput").value;
}

function getUser() {
    return document.getElementById("userInput").value;
}

connection.on("ReceiveMessage", 
    function (user, message) {
        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = user + " says " + msg;
        appendToMessagesList(encodedMsg);
    }
);

// Game-specific function
connection.on("ReceiveUpdatedGameState", 
    function (user, message) {
        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = user + " changed game state which is now: \n" + msg;
        
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.start().then(function () {
    sendButton.disabled = false;
}).catch(logError);

sendButton.addEventListener("click", 
    function (event) {
        var user = getUser();
        var message = getMessage();
        connection.invoke("ProcessActionAndSendUpdatedGameState", user, message).catch(logError);
        event.preventDefault();
    }
);

document.getElementById("actionCall").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();

    connection.invoke("UserDidCall", gameId, user).catch(logError);
    event.preventDefault();
});

document.getElementById("actionRaise").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    var amount = 20;

    connection.invoke("UserDidRaise", gameId, user, amount).catch(logError);
    event.preventDefault();
});

document.getElementById("actionFold").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();

    connection.invoke("UserDidFold", gameId, user).catch(logError);
    event.preventDefault();
});

