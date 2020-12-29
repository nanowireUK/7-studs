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
        roomId: window.decodeURIComponent(window.location.pathname.replace(/\//g, '')) || localStorage.getItem('roomId') || null,
        username: localStorage.getItem('username') || null,
        rejoinCode: localStorage.getItem('rejoinCode') || null,
        leaverCount: 0,
        awaitingResponse: false,
        muted: JSON.parse(localStorage.getItem('muted')) || false,
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
        setRoomId: (state, { payload }) => ({ ...state, roomId: payload }),
        setRejoinCode: (state, { payload }) => ({ ...state, rejoinCode: payload }),
        setLeaverCount: (state, { payload }) => ({ ...state, leaverCount: payload }),
        awaitingResponse: (state, { payload } ) => ({ ...state, awaitingResponse: payload}),
        setMuted: (state, { payload} ) => ({ ...state, muted: payload }),
    },
});

export const {
    connected,
    reconnecting,
    disconnected,
    setUsername,
    setRoomId,
    setRejoinCode,
    setLeaverCount,
    awaitingResponse,
    setMuted,
} = hubSlice.actions;

export const serverConnected = () => (dispatch, getState, connection) => {
    const { roomId, username, rejoinCode } = getState().hub;

    dispatch(connected());

    if (roomId !== null && username !== null && rejoinCode !== null) {
        dispatch(rejoin(roomId, username, rejoinCode));
    } else {
        console.log(roomId, username, rejoinCode)
    }
}

export const join = (roomId, username) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedJoin', roomId, username)
        .then(() => {
            window.history.pushState(null, '', window.encodeURIComponent(roomId))
            localStorage.setItem('roomId', roomId);
            localStorage.setItem('username', username);
            dispatch(setUsername(username));
        })
        .catch(console.log);
};

export const rejoin = (roomId, username, rejoinCode) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedRejoin', roomId, username, rejoinCode)
        .then(() => {
            window.history.pushState(null, '', window.encodeURIComponent(roomId))
            localStorage.setItem('roomId', roomId);
            localStorage.setItem('username', username);
            localStorage.setItem('rejoinCode', rejoinCode);
            dispatch(setUsername(username));
        })
        .catch(console.log);
};

export const sendServerAction = (serverMethod, ...args) => (dispatch, getState, connection) => {
    const { roomId, username } = getState().hub;
    dispatch(awaitingResponse(true));
    connection
        .invoke(serverMethod, roomId, username, ...args)
        .then(console.log)
        .catch(console.log);
}

export const sendServerActionWithLeaverCount = (serverMethod, ...args) => (dispatch, getState) => {
    const { leaverCount } = getState().hub;
    dispatch(sendServerAction(serverMethod, leaverCount.toString(), ...args));
}

export const selectRoomId = (state) => state.hub.roomId;

export const selectUsername = (state) => state.hub.username;

export const selectRejoinCode = (state) => state.hub.rejoinCode;

export const selectConnectionState = (state) => state.hub.connectionState;

export const selectMuted = (state) => state.hub.muted;

export const mute = () => (dispatch) => {
    localStorage.setItem('muted', true);
    dispatch(setMuted(true));
}

export const unmute = () => (dispatch) => {
    localStorage.setItem('muted', false);
    dispatch(setMuted(false));
}

export default hubSlice.reducer;
