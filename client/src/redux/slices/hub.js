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
        joinError: null,
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
        setJoinError: (state, { payload} ) => ({ ...state, joinError: payload }),
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
    setJoinError,
} = hubSlice.actions;

export const setHubJoinError = (e) => (dispatch, getState, connection) => {
    const [error, hubException = error] = e.toString().split('HubException: ');

    if (e === null) dispatch(setJoinError(null));
    else if (hubException === 'RoomAlreadyExists') dispatch(setJoinError('Room already exists, please try a different name'));
    else if (hubException === 'CurrentGameLimitExceeded') dispatch(setJoinError('Too many games in progress, please try again later'));
    else if (hubException === 'RoomDoesNotExist') dispatch(setJoinError('Couldn\'t find an existing room with that name'));
    else if (hubException === 'RoomNotAcceptingNewPlayers') dispatch(setJoinError('This room is currently private'));
    else if (hubException === 'RoomNotAcceptingSpectators') dispatch(setJoinError('This room is currently private'));
    else if (hubException === 'RoomIsFull') dispatch(setJoinError('This room is full, try spectating instead'));
    else if (hubException === 'CannotJoinGameInProgress') dispatch(setJoinError('Game in progress, please try again later'));
    else {
        console.log(error);
        dispatch(setJoinError('Something went wrong'));
    }
}

export const serverConnected = () => (dispatch, getState, connection) => {
    const { roomId, username, rejoinCode } = getState().hub;

    dispatch(connected());

    if (roomId !== null && username !== null && rejoinCode !== null) {
        dispatch(rejoin(roomId, username, rejoinCode));
    } else {
        console.log(roomId, username, rejoinCode)
    }
}

export const create = (roomId, username) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedCreateAndJoinRoom', roomId, username)
        .then(() => {
            window.history.pushState(null, '', window.encodeURIComponent(roomId))
            localStorage.setItem('roomId', roomId);
            localStorage.setItem('username', username);
            dispatch(setUsername(username));
            dispatch(setJoinError(null));
        })
        .catch(e => {
            dispatch(setHubJoinError(e));
        });
};

export const join = (roomId, username) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedJoinExistingRoom', roomId, username)
        .then(() => {
            window.history.pushState(null, '', window.encodeURIComponent(roomId));
            localStorage.setItem('roomId', roomId);
            localStorage.setItem('username', username);
            dispatch(setUsername(username));
            dispatch(setJoinError(null));
        })
        .catch(e => {
            dispatch(setHubJoinError(e));
        });
};

export const spectate = (roomId, username) => (dispatch, getState, connection) => {
    dispatch(awaitingResponse(true));
    connection
        .invoke('UserClickedSpectate', roomId, username)
        .then(() => {
            window.history.pushState(null, '', window.encodeURIComponent(roomId));
            localStorage.setItem('roomId', roomId);
            localStorage.setItem('username', username);
            dispatch(setUsername(username));
            dispatch(setJoinError(null));
        })
        .catch(e => {
            dispatch(setHubJoinError(e));
        });
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

export const selectJoinError = (state) => state.hub.joinError;

export const mute = () => (dispatch) => {
    localStorage.setItem('muted', true);
    dispatch(setMuted(true));
}

export const unmute = () => (dispatch) => {
    localStorage.setItem('muted', false);
    dispatch(setMuted(false));
}

export default hubSlice.reducer;
