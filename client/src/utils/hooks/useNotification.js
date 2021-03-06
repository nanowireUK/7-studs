import { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { Howl } from 'howler';

import notificationSrc from '../../assets/audio/notify.mp3';
import airhornSrc from '../../assets/audio/airhorn.mp3';

import { selectIsMyTurn } from '../../redux/slices/game';
import { selectMuted } from '../../redux/slices/hub';

export const useNotifications = () => {
    const isMyTurn = useSelector(selectIsMyTurn);
    const isMuted = useSelector(selectMuted);

    useEffect(() => {
        const notifySound = new Howl({
            src: [notificationSrc],
        });

        /* const airhornSound = new Howl({
            src: [airhornSrc],
        });

        const timeout = isMyTurn && !isMuted ? setTimeout(() => {
            airhornSound.play();
        }, 20000) : -1;
        */

        if (isMyTurn && !isMuted) notifySound.play();

        return () => {
            notifySound.pause();
            // airhornSound.pause();
            // clearTimeout(timeout);
        }
    }, [isMyTurn, isMuted]);
}