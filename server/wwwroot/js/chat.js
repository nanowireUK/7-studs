"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//var sendButton = document.getElementById("sendButton");
////Disable send button until connection is established
//sendButton.disabled = true;

// Original demo function

function appendToMessagesList(el) {
    var li = document.createElement("li");
    li.append(el);
    //document.getElementById("messagesList").appendChild(li);
    document.getElementById("messagesList").prepend(li);
}

function logError (err) {
    appendToMessagesList('Error: ' + err);
    return console.error(err.toString());
}

function getGameId() {
    return "7Studs Main Event"; // We'll set this from the URL at a later date
}

function getMessage() {
    return document.getElementById("actionModifiers").value;
}

function getUser() {
    return document.getElementById("userInput").value;
}

//connection.on("ReceiveMessage", 
//    function (user, message) {
//        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
//        var encodedMsg = user + " says " + msg;
//        appendToMessagesList(encodedMsg);
//    }
//);

// Game-specific function
connection.on("ReceiveUpdatedGameState", 
    function (gameState) {
        var msg = gameState.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = "Game state is currently: \n" + gameState;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.start().then(
    function () {
        actionJoin.disabled = false;
        actionCall.disabled = false;
        actionRaise.disabled = false;
        actionFold.disabled = false;
    }
).catch(logError);

//sendButton.addEventListener("click", 
//   function (event) {
//        var user = getUser();
//        var message = getMessage();
//        connection.invoke("ProcessActionAndSendUpdatedGameState", user, message).catch(logError);
//        event.preventDefault();
//    }
//);

// --------------- JOIN

document.getElementById("actionJoin").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        connection.invoke("UserClickedJoin", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- START

document.getElementById("actionStart").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        connection.invoke("UserClickedStart", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- RAISE

document.getElementById("actionRaise").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    var amount = getMessage();
    connection.invoke("UserClickedRaise", gameId, user, amount).catch(logError);
    event.preventDefault();
});

// --------------- CALL

document.getElementById("actionCall").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedCall", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- CHECK

document.getElementById("actionCheck").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedCheck", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- FOLD

document.getElementById("actionFold").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedFold", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- COVER

document.getElementById("actionCover").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedCover", gameId, user).catch(logError);
    event.preventDefault();
});



