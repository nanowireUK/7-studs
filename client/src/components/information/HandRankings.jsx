import React from 'react';
import { Box, Heading, Table, TableBody, TableCell, TableRow, Text } from "grommet";
import PokerCard from '../PokerCard';

export default function HandRankings () {
    return <Box gap="small">
        <Heading margin="none" level={2}>Hand Rankings</Heading>
        <Table>
            <TableBody>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Straight Flush</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="4" suit="H"/>
                            <PokerCard face="5" suit="H"/>
                            <PokerCard face="6" suit="H"/>
                            <PokerCard face="7" suit="H"/>
                            <PokerCard face="8" suit="H"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>0.03%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Four of a Kind</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="A" suit="H"/>
                            <PokerCard face="A" suit="D"/>
                            <PokerCard face="A" suit="C"/>
                            <PokerCard face="A" suit="S"/>
                            <PokerCard face="4" suit="D"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>0.17%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Full House</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="8" suit="H"/>
                            <PokerCard face="8" suit="D"/>
                            <PokerCard face="8" suit="C"/>
                            <PokerCard face="K" suit="H"/>
                            <PokerCard face="K" suit="S"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>2.60%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Flush</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="10" suit="C"/>
                            <PokerCard face="4" suit="C"/>
                            <PokerCard face="Q" suit="C"/>
                            <PokerCard face="7" suit="C"/>
                            <PokerCard face="2" suit="C"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>3.03%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Straight</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="7" suit="C"/>
                            <PokerCard face="8" suit="H"/>
                            <PokerCard face="9" suit="D"/>
                            <PokerCard face="10" suit="H"/>
                            <PokerCard face="J" suit="S"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>4.62%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Three of a Kind</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="Q" suit="H"/>
                            <PokerCard face="Q" suit="C"/>
                            <PokerCard face="Q" suit="D"/>
                            <PokerCard face="5" suit="S"/>
                            <PokerCard face="A" suit="D"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>4.83%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Two Pair</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="3" suit="H"/>
                            <PokerCard face="3" suit="D"/>
                            <PokerCard face="6" suit="C"/>
                            <PokerCard face="6" suit="H"/>
                            <PokerCard face="K" suit="S"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>23.5%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>Pair</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="5" suit="H"/>
                            <PokerCard face="5" suit="S"/>
                            <PokerCard face="2" suit="C"/>
                            <PokerCard face="J" suit="C"/>
                            <PokerCard face="A" suit="D"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>43.8%</Text></TableCell>
                </TableRow>
                <TableRow>
                    <TableCell scope="row" direction="column" gap="small">
                        <Text>High Card</Text>
                        <Box direction="row" height="35px" gap="xxsmall">
                            <PokerCard face="2" suit="D"/>
                            <PokerCard face="5" suit="S"/>
                            <PokerCard face="6" suit="S"/>
                            <PokerCard face="J" suit="H"/>
                            <PokerCard face="A" suit="C"/>
                        </Box>
                    </TableCell>
                    <TableCell scope="row" direction="column" gap="small"><Text>17.4%</Text></TableCell>
                </TableRow>
            </TableBody>
        </Table>
        <br></br>
    </Box>
}