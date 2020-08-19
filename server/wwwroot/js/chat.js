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

function getModifiers() {
    return document.getElementById("actionModifiers").value;
}

function getUser() {
    return document.getElementById("userInput").value;
}

function getRejoinCode() {
    return document.getElementById("rejoinCode").value;
}

// Game-specific function
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
        connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Join */ 1, gameId, user, "").catch(logError);
        //        public async Task UserClickedActionButton(ActionEnum actionType, string gameId, string user, string amount)
        event.preventDefault();
    }
);

// --------------- REJOIN

document.getElementById("actionRejoin").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        var code = getRejoinCode();
        connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Rejoin */ 2, gameId, user, code).catch(logError);
        //        public async Task UserClickedActionButton(ActionEnum actionType, string gameId, string user, string amount)
        event.preventDefault();
    }
);

// --------------- START

document.getElementById("actionStart").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Start */ 4, gameId, user, "").catch(logError);
        event.preventDefault();
    }
);

// --------------- CHECK

document.getElementById("actionCheck").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Check */ 10, gameId, user, "").catch(logError);
    event.preventDefault();
});

// --------------- CALL

document.getElementById("actionCall").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Call */ 11, gameId, user, "").catch(logError);
    event.preventDefault();
});

// --------------- RAISE

document.getElementById("actionRaise").addEventListener("click", 
    function (event) {
        var gameId = getGameId();    
        var user = getUser();
        var amount = getModifiers();
        connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Raise */ 12, gameId, user, amount).catch(logError);
        event.preventDefault();
    }
);

// --------------- COVER

document.getElementById("actionCover").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Cover */ 13, gameId, user, "").catch(logError);
    event.preventDefault();
});

// --------------- FOLD

document.getElementById("actionFold").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Fold */ 14, gameId, user, "").catch(logError);
    event.preventDefault();
});


// --------------- Get Game State (test feature)

document.getElementById("actionGetState").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser(); 
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.GetState */ 20, gameId, user, "").catch(logError);
    event.preventDefault();
});

// --------------- Get Game Log (test feature)

document.getElementById("actionGetLog").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();  
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.GetLog */ 21, gameId, user, "").catch(logError);
    event.preventDefault();
});

// --------------- Reply game from game log (test feature)

document.getElementById("actionReplay").addEventListener("click", function (event) {
    var gameId = getGameId();    
    var user = getUser();
    var gameLog = getModifiers();   
    connection.invoke("UserClickedActionButton", /* SevenStuds.Models.ActionEnum.Replay */ 22, gameId, user, gameLog).catch(logError);
    event.preventDefault();
});
