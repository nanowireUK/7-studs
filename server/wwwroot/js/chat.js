"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

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

function getModifiers() {
    return document.getElementById("actionModifiers").value;
}

function getUser() {
    return document.getElementById("userInput").value;
}

function getRejoinCode() {
    return document.getElementById("rejoinCode").value;
}

// -------------------------------------------------------------------------------------------------------------
// Functions that the server can call and that we have to handle

connection.on("ReceiveMyGameState", 
    function (gameState) {
        var msg = gameState.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = "Game state from my perspective is currently: \n" + gameState;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveGameLog", 
    function (gameLog) {
        var msg = gameLog.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = "Game log: \n" + gameLog;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveOverallGameState", 
    function (gameState) {
        var msg = gameState.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var encodedMsg = "Overall game state: \n" + gameState;
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
        actionCover.disabled = false;
    }
).catch(logError);

// -------------------------------------------------------------------------------------------------------------
// Client-side actions that we want to pass to the server

// --------------- JOIN

document.getElementById("actionJoin").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        connection.invoke("UserClickedJoin", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- REJOIN

document.getElementById("actionRejoin").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        var rejoinCode = getRejoinCode();
        connection.invoke("UserClickedRejoin", gameId, user, rejoinCode).catch(logError);
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

// --------------- CHECK

document.getElementById("actionCheck").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedCheck", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- CALL

document.getElementById("actionCall").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedCall", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- RAISE

document.getElementById("actionRaise").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        var raiseAmount = getModifiers();
        connection.invoke("UserClickedRaise", gameId, user, raiseAmount).catch(logError);
        event.preventDefault();
    }
);

// --------------- COVER

document.getElementById("actionCover").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedActionCover", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- FOLD

document.getElementById("actionFold").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedFold", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- Get Game State (test feature)

document.getElementById("actionGetState").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser(); 
    connection.invoke("UserClickedGetState", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- Get Game Log (test feature)

document.getElementById("actionGetLog").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();  
    connection.invoke("UserClickedGetLog", gameId, user).catch(logError);
    event.preventDefault();
});

// --------------- Replay game from game log (test feature)

document.getElementById("actionReplay").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    var gameLog = getModifiers();   
    connection.invoke("UserClickedReplay", gameId, user, gameLog).catch(logError);
    event.preventDefault();
});
