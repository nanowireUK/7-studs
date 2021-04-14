import { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Howl } from 'howler';

import notificationSrc from '../../assets/audio/notify.mp3';

import { selectIsMyTurn } from '../../redux/slices/game';
import { selectMuted } from '../../redux/slices/hub';

export const useNotifications = () => {
    const isMyTurn = useSelector(selectIsMyTurn);
    const isMuted = useSelector(selectMuted);

    useEffect(() => {
        const notifySound = new Howl({
            src: [notificationSrc],
        });

        if (isMyTurn && !isMuted) notifySound.play();

        return () => {
            notifySound.pause();
            // airhornSound.pause();
            // clearTimeout(timeout);
        };
    }, [isMyTurn, isMuted]);
};
