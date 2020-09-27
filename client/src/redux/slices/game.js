import { createSlice } from '@reduxjs/toolkit';
import { awaitingResponse, setRejoinCode } from './hub';

export const gameSlice = createSlice({
    name: 'game',
    initialState: null,
    reducers: {
        updateGame: (state, { payload }) => (payload),
    },
});

export const {
    updateGame,
} = gameSlice.actions;


export const start = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedStart', gameId, username)
        .then(console.log)
        .catch(console.log);
}

export const leave = () => (dispatch) => {
    localStorage.setItem('rejoinCode', '');
    dispatch(setRejoinCode(''));
    dispatch(updateGame(null));
}

export const raise = (raiseAmount) => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedRaise', gameId, username, raiseAmount)
        .then(console.log)
        .catch(console.log);
}

export const check = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedCheck', gameId, username)
        .then(console.log)
        .catch(console.log);
};

export const fold = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedFold', gameId, username)
        .then(console.log)
        .catch(console.log);
};

export const cover = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke("UserClickedCover", gameId, username)
        .then(console.log)
        .catch(console.log);
};

export const call = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke("UserClickedCall", gameId, username)
        .then(console.log)
        .catch(console.log);
};

export const reveal = () => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke("UserClickedReveal", gameId, username)
        .then(console.log)
        .catch(console.log);
};

export const selectGame = (state) => state.game;

export const selectInLobby = (state) =>
    state.game !== null && state.game.GameMode === 'LobbyOpen';
export const selectHandInProgress = (state) =>
    state.game !== null && state.game.GameMode === 'HandInProgress';
export const selectHandsBeingRevealed = (state) =>
    state.game !== null && state.game.GameMode === 'HandsBeingRevealed';
export const selectHandCompleted = (state) =>
    state.game !== null && state.game.GameMode === 'HandCompleted';

export const selectIsAdmin = (state) =>
    state.game !== null && state.game.IAmAdministrator;

export const selectPlayers = (state) =>
           (state.game !== null ? state.game.PlayerViewOfParticipants : []).map(
               (
                   { Name: name, UncommittedChips: chips, Cards: cards, IsCurrentPlayer: isCurrentPlayer, IsMe: isMe, IsAdmin: isAdmin, IsDealer: isDealer, VisibleHandDescription: handDescription },
               ) => ({
                   name,
                   chips,
                   cards,
                   handDescription,
                   isMe,
                   isCurrentPlayer,
                   isDealer,
                   isAdmin,
               })
           );

export const selectPots = (state) => state.game.Pots;

export const selectGameStatus = (state) => state.game.StatusMessage;

export const selectCanDoAction = (action) => (state) =>
    state.game !== null && state.game.AvailableActions.includes(action);

export const selectAnte = (state) => state.game.Ante;

export const selectMaxRaise = (state) => state.game.MyMaxRaise;

export const PlayerActions = Object.freeze({
    START: 'Start',
    CHECK: 'Check',
    CALL: 'Call',
    COVER: 'Cover',
    RAISE: 'Raise',
    FOLD: 'Fold',
    REVEAL: 'Reveal'
});

export default gameSlice.reducer;
