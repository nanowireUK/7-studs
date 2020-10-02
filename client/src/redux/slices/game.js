import { createSlice } from '@reduxjs/toolkit';
import { sendServerAction, sendServerActionWithLeaverCount } from './hub';

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

export const start = () => sendServerActionWithLeaverCount('UserClickedStart');
export const leave = () => sendServerAction('UserClickedLeave');

export const raise = (raiseAmount) => sendServerActionWithLeaverCount('UserClickedRaise', raiseAmount);
export const check = () => sendServerActionWithLeaverCount('UserClickedCheck');
export const cover = () => sendServerActionWithLeaverCount('UserClickedCover');
export const call = () => sendServerActionWithLeaverCount('UserClickedCall');
export const fold = () => sendServerActionWithLeaverCount('UserClickedFold');
export const reveal = () => sendServerActionWithLeaverCount('UserClickedReveal');

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
                   { Name: name, UncommittedChips: chips, Cards: cards, IsCurrentPlayer: isCurrentPlayer, IsMe: isMe, IsAdmin: isAdmin, IsDealer: isDealer, IsOutOfThisGame: isOutOfThisGame, HasFolded: hasFolded, VisibleHandDescription: handDescription },
               ) => ({
                   name,
                   chips,
                   cards,
                   handDescription,
                   isMe,
                   isCurrentPlayer,
                   isDealer,
                   isAdmin,
                   isOutOfThisGame,
                   hasFolded
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

export const GameModes = Object.freeze({
    LOBBY_OPEN: 'LobbyOpen',
    HAND_IN_PROGRESS: 'HandInProgress',
    HANDS_BEING_REVEALED: 'HandsBeingRevealed',
    HAND_COMPLETED: 'HandCompleted'
});

export default gameSlice.reducer;
