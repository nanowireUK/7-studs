import React, { useRef } from 'react';

import { Box } from 'grommet';

import {ReactComponent as Heart} from '../assets/suit-hearts.svg';
import {ReactComponent as Club} from '../assets/suit-clubs.svg';
import {ReactComponent as Diamond} from '../assets/suit-diamonds.svg';
import {ReactComponent as Spade} from '../assets/suit-spades.svg';

import { useContainerDimensions } from '../utils/hooks';

function Face ({ face = '', suit = '', invisibleToOthers = false }) {
    const color = ['H', 'D'].includes(suit.toUpperCase()) ? '#d40000' : 'black';
    return (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" version="1.0">
            <text x="50%" y="80%" textAnchor="middle" fontFamily="Bellota Text" fontWeight="700" fill={color} opacity={invisibleToOthers ? '50%' : null}>{face === 'T' ? '10' : face}</text>
        </svg>
    )
}

function Suit ({ suit, invisibleToOthers = false }) {
    if (suit) {
        switch (suit.toUpperCase()) {
            case 'S': return <Spade opacity={invisibleToOthers ? '50%' : null}/>
            case 'D': return <Diamond opacity={invisibleToOthers ? '50%' : null}/>
            case 'H': return <Heart opacity={invisibleToOthers ? '50%' : null}/>
            case 'C': return <Club opacity={invisibleToOthers ? '50%' : null}/>
            default: break;
        }
    }
    return <Face invisibleToOthers={invisibleToOthers} face="?" suit={suit}/>
}

export default function PokerCard ({ face, suit, invisibleToOthers = false }) {
    const cardRef = useRef(null);
    const { height } = useContainerDimensions(cardRef);

    if (face === '?' || suit === '?') return (<Box ref={cardRef} direction="row" title={face !== '?' ? `${face}${suit}` : 'Hidden'} elevation="xsmall" pad="xsmall" border round="xsmall" gap="xsmall" background={{
        image: "url(https://upload.wikimedia.org/wikipedia/commons/3/30/Card_back_05a.svg)", // need to double check license and possibly find suitable alternative if appropriate (LGPL-2)
        size: "115%"
    }}>
        <Box pad="xsmall" direction="column" align="center" justify="around" width={`${height + 2}px`} testborder={{ color: 'blue', style: 'dashed' }}></Box>
    </Box>)

    return (
        <Box fill="vertical" ref={cardRef} direction="row" title={face !== '?' ? `${face}${suit}` : 'Hidden'} elevation="xsmall" pad="xsmall" border round="xsmall" gap="2px" background="white">
            <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'blue', style: 'dashed' }}><Face invisibleToOthers={invisibleToOthers} face={face} suit={suit}/></Box>
            <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'red', style: 'dashed' }}><Suit invisibleToOthers={invisibleToOthers} suit={suit} /></Box>
        </Box>
    );
}