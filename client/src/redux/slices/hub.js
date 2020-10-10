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
        leaverCount: 0,
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
        setUsername: (state, { payload }) => ({ ...state, username: payload }),
        setGameId: (state, { payload }) => ({ ...state, gameId: payload }),
        setRejoinCode: (state, { payload }) => ({ ...state, rejoinCode: payload }),
        setLeaverCount: (state, { payload }) => ({ ...state, leaverCount: payload }),
        awaitingResponse: (state, { payload } ) => ({ ...state, awaitingResponse: payload})
    },
});

export const {
    connected,
    reconnecting,
    disconnected,
    setUsername,
    setGameId,
    setRejoinCode,
    setLeaverCount,
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
            setUsername(username);
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
            setUsername(username);
        })
        .catch(console.log);
};

export const sendServerAction = (serverMethod, ...args) => (dispatch, getState, connection) => {
    const { gameId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke(serverMethod, gameId, username, ...args)
        .then(console.log)
        .catch(console.log);
}

export const sendServerActionWithLeaverCount = (serverMethod, ...args) => (dispatch, getState) => {
    const { leaverCount } = getState().hub;
    dispatch(sendServerAction(serverMethod, leaverCount.toString(), ...args));
}

export const selectGameId = (state) => state.hub.gameId;

export const selectUsername = (state) => state.hub.username;

export const selectRejoinCode = (state) => state.hub.rejoinCode;

export const selectConnectionState = (state) => state.hub.connectionState;

export default hubSlice.reducer;
