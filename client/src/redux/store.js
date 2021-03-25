import { configureStore } from '@reduxjs/toolkit';

import signalR, { connection } from './helpers/signalR';
import logger from './helpers/logger';
import rootReducer from './rootReducer';

const store = configureStore({
    reducer: rootReducer,
    middleware: (getDefaultMiddleware) => getDefaultMiddleware({
        thunk: {
            extraArgument: connection,
        },
    }).concat(logger, signalR),
});

if (process.env.NODE_ENV === 'development' && module.hot) {
    module.hot.accept('./rootReducer', () => {
        const newRootReducer = require('./rootReducer').default;
        store.replaceReducer(newRootReducer);
    });
}

export default store;
