
import gameReducer from './slices/game';
import hubReducer from './slices/hub';
import viewReducer from './slices/views';
import { combineReducers } from '@reduxjs/toolkit';

export default combineReducers({
    game: gameReducer,
    hub: hubReducer,
    views: viewReducer,
})