import { createSlice } from '@reduxjs/toolkit';

export const ConnectionState = Object.freeze({
    CONNECTED: 'CONNECTED',
    RECONNECTING: 'RECONNECTING',
    DISCONNECTED: 'DISCONNECTED',
});

export const hubSlice = createSlice({
    name: 'hub',
    initialState: {
        connectionState: ConnectionState.RECONNECTING,
        gameId: localStorage.getItem('gameId') || null,
        username: localStorage.getItem('username') || null,
        rejoinCode: localStorage.getItem('rejoinCode') || null,
        awaitingResponse: false,
    },
    reducers: {
        connected: (state) => {
            state.connectionState = ConnectionState.CONNECTED;
        },
        reconnecting: (state) => {
            state.connectionState = ConnectionState.RECONNECTING;
        },
        disconnected: (state) => {
            state.connectionState = ConnectionState.DISCONNECTED;
        },
        joinedGame: (state, { payload: { gameId, username } }) => ({
            ...state,
            gameId,
            username,
        }),
        setRejoinCode: (state, { payload }) => ({
            ...state,
            rejoinCode: payload,
        }),
        awaitingResponse: (state, { payload } ) => ({ ...state, awaitingResponse: payload})
    },
});

export const {
    connected,
    reconnecting,
    disconnected,
    joinedGame,
    setRejoinCode,
    awaitingResponse,
} = hubSlice.actions;

export const serverConnected = () => (dispatch, getState, connection) => {
    const { gameId, username, rejoinCode } = getState().hub;

    dispatch(connected());

    if (gameId !== null && username !== null && rejoinCode !== null) {
        dispatch(rejoin(gameId, username, rejoinCode));
    } else {
        console.log(gameId, username, rejoinCode)
    }
}

export const join = (gameId, username) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedJoin', gameId, username)
        .then(() => {
            console.log('User clicked join')
            localStorage.setItem('gameId', gameId);
            localStorage.setItem('username', username);
            dispatch(joinedGame(({ gameId, username })));
        })
        .catch(console.log);
};

export const rejoin = (gameId, username, rejoinCode) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedRejoin', gameId, username, rejoinCode)
        .then(() => {
            localStorage.setItem('gameId', gameId);
            localStorage.setItem('username', username);
            localStorage.setItem('rejoinCode', rejoinCode);
            dispatch(joinedGame(({ gameId, username })));
        })
        .catch(console.log);
};

export const selectGameId = (state) => state.hub.gameId;

export const selectUsername = (state) => state.hub.username;

export const selectRejoinCode = (state) => state.hub.rejoinCode;

export const selectConnectionState = (state) => state.hub.connectionState;

export default hubSlice.reducer;
