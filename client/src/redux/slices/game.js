import { createSlice } from '@reduxjs/toolkit';

export const ConnectionState = Object.freeze({
    CONNECTED: 'CONNECTED',
    RECONNECTING: 'RECONNECTING',
    DISCONNECTED: 'DISCONNECTED'
});

export const gameSlice = createSlice({
    name: 'game',
    initialState: {
        latest: {},
        connectionState: ConnectionState.RECONNECTING,
        awaitingResponse: false,
        gameId: null,
        reconnectId: null,
    },
    reducers: {
        updateGame: (state, { payload }) => { state.latest = payload },
        awaitingResponse: (state, { payload }) => { state.awaitingResponse = payload },
        connected: (state) => { state.connectionState = ConnectionState.CONNECTED },
        reconnecting: (state) => { state.connectionState = ConnectionState.RECONNECTING },
        disconnected: (state) => { state.connectionState = ConnectionState.DISCONNECTED },
    },
});

export const { updateGame, awaitingResponse, connected, reconnecting, disconnected } = gameSlice.actions;

export const join = (gameId, user) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true))
    connection.invoke('UserClickedJoin', gameId, user);
}

export const selectGame = state => state.game;

export default gameSlice.reducer;
