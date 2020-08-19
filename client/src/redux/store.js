import { configureStore } from '@reduxjs/toolkit';
import gameReducer from './slices/game';

import signalR, { connection } from './helpers/signalR';
import logger from './helpers/logger';

export default configureStore({
    reducer: {
        game: gameReducer,
    },
    middleware: getDefaultMiddleware => getDefaultMiddleware(({
        thunk: { 
            extraArgument: connection
        },
    })).concat(logger, signalR)
});

