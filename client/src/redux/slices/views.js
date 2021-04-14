import { createSlice } from '@reduxjs/toolkit';

export const Views = Object.freeze({
    WELCOME: 'WELCOME',
    CREATE_ROOM: 'CREATE_ROOM',
    JOIN_ROOM: 'JOIN_ROOM',
    IN_GAME: 'IN_GAME',
});

export const viewSlice = createSlice({
    name: 'views',
    initialState: {
        currentView: window.decodeURIComponent(window.location.pathname.replace(/\//g, '')) ? Views.JOIN_ROOM : Views.WELCOME,
    },
    reducers: {
        setCurrentView: (state, { payload }) => ({ ...state, currentView: payload }),
    },
});

export const {
    setCurrentView,
} = viewSlice.actions;

export const selectCurrentView = (state) => state.views.currentView;

export default viewSlice.reducer;
