import { createSlice } from '@reduxjs/toolkit';

export const lobbySlice = createSlice({
    name: 'lobby',
    initialState: null,
    reducers: {
        setLobbySettings: (state, { payload }) => payload,
    },
});

export const {
    setLobbySettings,
} = lobbySlice.actions;

const updateLobbySettings = (newSettings) => (dispatch, getState, connection) => {
    const { hub: { roomId, username }, lobby } = getState();

    console.log({
        ...lobby,
        ...newSettings,
    });

    connection
        .invoke('UserClickedUpdateLobbySettings', roomId, username, JSON.stringify({
            ...lobby,
            ...newSettings,
        }))
        .then(console.log)
        .catch(console.log);
};

export const updateAcceptNewPlayers = (AcceptNewPlayers) => updateLobbySettings({ AcceptNewPlayers });
export const updateAllowSpectators = (AcceptNewSpectators) => updateLobbySettings({ AcceptNewSpectators });
export const updateAnte = (Ante) => updateLobbySettings({ Ante });
export const updateInitialChipQuantity = (InitialChipQuantity) => updateLobbySettings({ InitialChipQuantity });
export const updateIsLimitGame = (IsLimitGame) => updateLobbySettings({
    IsLimitGame, LimitGameBringInAmount: 2, LimitGameSmallBet: 5, LimitGameBigBet: 10, LimitGameMaxRaises: 4,
});
export const updateLowestCardPlacesFirstBet = (LowestCardPlacesFirstBet) => updateLobbySettings({ LowestCardPlacesFirstBet });
export const updateHideFoldedCards = (HideFoldedCards) => updateLobbySettings({ HideFoldedCards });

export const selectAcceptNewPlayers = (state) => state.lobby.AcceptNewPlayers;
export const selectAllowSpectators = (state) => state.lobby.AcceptNewSpectators;
export const selectAnte = (state) => state.lobby.Ante;
export const selectInitialChipQuantiy = (state) => state.lobby.InitialChipQuantity;
export const selectIsLimitGame = (state) => state.lobby.IsLimitGame;
export const selectLimitGameBigBet = (state) => state.lobby.LimitGameBigBet;
export const selectLimitGameBringInAmount = (state) => state.lobby.LimitGameBringInAmount;
export const selectLimitGameMaxRaises = (state) => state.lobby.LimitGameMaxRaises;
export const selectLimitGameSmallBet = (state) => state.lobby.LimitGameSmallBet;
export const selectLowestCardPlacesFirstBet = (state) => state.lobby.LowestCardPlacesFirstBet;
export const selectHideFoldedCards = (state) => state.lobby.HideFoldedCards;

export default lobbySlice.reducer;
