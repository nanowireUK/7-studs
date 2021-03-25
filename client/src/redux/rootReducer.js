import { combineReducers } from '@reduxjs/toolkit';
import gameReducer from './slices/game';
import hubReducer from './slices/hub';
import viewReducer from './slices/views';
import lobbyReducer from './slices/lobby';

export default combineReducers({
    game: gameReducer,
    hub: hubReducer,
    views: viewReducer,
    lobby: lobbyReducer,
});
