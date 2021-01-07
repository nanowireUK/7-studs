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
export const proceed = () => sendServerActionWithLeaverCount('UserClickedContinue');
export const open = () => sendServerActionWithLeaverCount('UserClickedOpen');
export const leave = () => sendServerAction('UserClickedLeave');

export const raise = (raiseAmount) => sendServerActionWithLeaverCount('UserClickedRaise', raiseAmount);
export const check = () => sendServerActionWithLeaverCount('UserClickedCheck');
export const cover = () => sendServerActionWithLeaverCount('UserClickedCover');
export const call = () => sendServerActionWithLeaverCount('UserClickedCall');
export const fold = () => sendServerActionWithLeaverCount('UserClickedFold');
export const reveal = () => sendServerActionWithLeaverCount('UserClickedReveal');
export const goBlind = () => sendServerActionWithLeaverCount('UserClickedBlindIntent');
export const revealBlind = () => sendServerActionWithLeaverCount('UserClickedBlindReveal');

export const selectGame = (state) => state.game;

export const selectMyHandDescription = (state) => state.game.MyHandDescription;

export const selectCurrentGameStandings = (state) =>
    (state.game !== null ? state.game.LobbyData.CurrentGameStandings : []).map(
        ({
            PlayerName: name,
            HasLeftRoom: hasLeftRoom,
            Status: status,
            RemainingFunds: remainingFunds
        }) => ({
            name,
            hasLeftRoom,
            status,
            remainingFunds
        })
    );

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

export const selectIntendsToPlayBlind = (state) =>
    state.game !== null && state.game.IIntendToPlayBlindInNextHand;

export const selectPlayingBlind = (state) =>
    state.game !== null && state.game.IAmPlayingBlindInCurrentHand;

export const selectPlayers = (state) =>
           (state.game !== null ? state.game.PlayerViewOfParticipants : []).map(
               (
                   {
                       Name: name,
                       UncommittedChips: chips,
                       Cards: cards,
                       IsCurrentPlayer: isCurrentPlayer,
                       IsMe: isMe,
                       IsAdmin: isAdmin,
                       IsDealer: isDealer,
                       IsOutOfThisGame: isOutOfThisGame,
                       HasFolded: hasFolded,
                       VisibleHandDescription: handDescription,
                       IsSharingHandDetails: isSharingHandDetails,
                       GainOrLossInLastHand: gainOrLossInLastHand,
                       HandsWon: handsWon,
                       LastActionInThisHand: lastActionInHand,
                       LastActionAmount: lastActionAmount,
                       RoundNumberOfLastAction: roundNumberOfLastAction,
                       IsPlayingBlindInCurrentHand: isPlayingBlind,
                    },
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
                   hasFolded,
                   isSharingHandDetails,
                   gainOrLossInLastHand,
                   handsWon,
                   lastActionInHand: roundNumberOfLastAction === state.game.RoundNumber ? lastActionInHand : '',
                   lastActionAmount: roundNumberOfLastAction === state.game.RoundNumber ? lastActionAmount : 0,
                   isPlayingBlind
               })
           );

export const selectAdminName = (state) => selectPlayers(state).find(({ isAdmin }) => isAdmin).name;

export const selectPots = (state) => state.game.Pots;

export const selectLastHandResult = (state) => (state.game.MostRecentHandResult || [])
    .map(potResult => potResult.map(({
        AmountWonOrLost: takeaway,
        PlayerName: name,
        Result: resultDescription,
        Stake: stake,
    }) => ({
        name,
        resultDescription,
        takeaway,
        stake
    })));

export const selectGameStatus = (state) => state.game.StatusMessage;

export const selectCanDoAction = (action) => (state) =>
    state.game !== null && state.game.AvailableActions.includes(action);

export const selectAnte = (state) => state.game.Ante;

export const selectMaxRaise = (state) => state.game.MyMaxRaise;

export const selectCallAmount = (state) => state.game.MyCallAmount;

export const selectCommunityCard = (state) => state.game.CommunityCard;

export const selectActionReference = (state) => `${state.game.RoomId}-${state.game.GameNumber}.${state.game.HandsPlayedIncludingCurrent}.${state.game.ActionNumber}`;

export const selectIsMyTurn = (state) => state.game.IsMyTurn;

export const PlayerActions = Object.freeze({
    START: 'Start',
    CHECK: 'Check',
    CALL: 'Call',
    COVER: 'Cover',
    RAISE: 'Raise',
    FOLD: 'Fold',
    REVEAL: 'Reveal',
    CONINUE: 'Continue',
    BLIND_INTENT: 'BlindIntent',
    BLIND_REVEAL: 'BlindReveal',
});

export const GameModes = Object.freeze({
    LOBBY_OPEN: 'LobbyOpen',
    HAND_IN_PROGRESS: 'HandInProgress',
    HANDS_BEING_REVEALED: 'HandsBeingRevealed',
    HAND_COMPLETED: 'HandCompleted',
});

export default gameSlice.reducer;
