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
    return document.getElementById("userInputGameId").value; // Was "7Studs Main Event" ... we'll look at setting this from the URL at a later date
}

function getModifiers() {
    return document.getElementById("actionModifiers").value;
}

function getUser() {
    return document.getElementById("userInput").value;
}

function getLeaverCount() {
    return document.getElementById("leaverCount").value;
}

// -------------------------------------------------------------------------------------------------------------
// Functions that the server can call and that we have to handle

connection.on("ReceiveMyGameState",
    function (gameState) {
        var game = JSON.parse(gameState);
        document.getElementById("leaverCount").value = game.CountOfLeavers;
        var encodedMsg = "Game state from my perspective is currently: \n" + gameState;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveGameLog",
    function (gameLog) {
        var encodedMsg = "Game log: \n" + gameLog;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveOverallGameState",
    function (gameState) {
        var encodedMsg = "Overall game state: \n" + gameState;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveLeavingConfirmation",
    function (gameState) {
        var encodedMsg = "Leaving confirmation: \n" + gameState;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.on("ReceiveAdHocServerData",
    function (serverData) {
        var encodedMsg = "Ad hoc server data: \n" + serverData;
        var pre = document.createElement("pre");
        pre.textContent = encodedMsg;
        appendToMessagesList(pre);
    }
);

connection.start().catch(logError);

// -------------------------------------------------------------------------------------------------------------
// Client-side actions that we want to pass to the server

// --------------- OPEN

document.getElementById("actionOpen").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedOpen", gameId, user, leaverCount).catch(logError);
        event.preventDefault();
    }
);

// --------------- JOIN

document.getElementById("actionJoin").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedJoin", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- SPECTATE

document.getElementById("actionSpectate").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedSpectate", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- LEAVE

document.getElementById("actionLeave").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedLeave", gameId, user).catch(logError);
        event.preventDefault();
    }
);

// --------------- REJOIN

document.getElementById("actionRejoin").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        var rejoinCode = getModifiers();
        connection.invoke("UserClickedRejoin", gameId, user, rejoinCode).catch(logError);
        event.preventDefault();
    }
);

// --------------- START

document.getElementById("actionStart").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedStart", gameId, user, leaverCount).catch(logError);
        event.preventDefault();
    }
);

// --------------- CONTINUE

document.getElementById("actionContinue").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedContinue", gameId, user, leaverCount).catch(logError);
        event.preventDefault();
    }
);

// --------------- REVEAL HAND

document.getElementById("actionReveal").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedReveal", gameId, user, leaverCount).catch(logError);
        event.preventDefault();
    }
);

// --------------- CHECK

document.getElementById("actionCheck").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedCheck", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- CALL

document.getElementById("actionCall").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedCall", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- RAISE

document.getElementById("actionRaise").addEventListener("click",
    function (event) {
        var gameId = getGameId();
        var user = getUser();
        var raiseAmount = getModifiers();
        var leaverCount = getLeaverCount();
        connection.invoke("UserClickedRaise", gameId, user, leaverCount, raiseAmount).catch(logError);
        event.preventDefault();
    }
);

// --------------- COVER

document.getElementById("actionCover").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedCover", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- FOLD

document.getElementById("actionFold").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedFold", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- Get Game State (test feature)

document.getElementById("actionGetState").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedGetState", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- Get Game Log (test feature)

document.getElementById("actionGetLog").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedGetLog", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- Get Game Log (test feature)

document.getElementById("actionAdHocQuery").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    var queryNum = getModifiers();
    connection.invoke("UserClickedAdHocQuery", gameId, user, leaverCount, queryNum).catch(logError);
    event.preventDefault();
});


// --------------- Replay game from game log (test feature)

document.getElementById("actionReplay").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    var gameLog = getModifiers();
    connection.invoke("UserClickedReplay", gameId, user, leaverCount, gameLog).catch(logError);
    event.preventDefault();
});

// --------------- Get My Game State (test feature)

document.getElementById("actionGetMyState").addEventListener("click", function (event) {
    var gameId = getGameId();
    var user = getUser();
    var leaverCount = getLeaverCount();
    connection.invoke("UserClickedGetMyState", gameId, user, leaverCount).catch(logError);
    event.preventDefault();
});

// --------------- Test button (used for experimentation)

document.getElementById("jdTest").addEventListener("click", function (event) {
    var gameLog = getModifiers();
    var msg = "Length of modifiers field is "+gameLog.length
    var escapedMsg = msg.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var pre = document.createElement("pre");
    pre.textContent = escapedMsg;
    appendToMessagesList(pre);
    event.preventDefault();
});
