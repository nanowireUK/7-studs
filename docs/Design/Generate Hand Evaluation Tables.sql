--
-- Generate hand evaluation tables
--

-------------------------------------------------------------------------------------------
-- (1) Set up ranks, suits and cards

IF object_id('tempdb..#ranks') IS NOT NULL DROP TABLE #ranks

create table #ranks (
	rank_code varchar(2),
	rank_name varchar(10),
	rank_prime int
)

insert #ranks values('2', 'Two', '2')
insert #ranks values('3', 'Three', '3')
insert #ranks values('4', 'Four', '5')
insert #ranks values('5', 'Five', '7')
insert #ranks values('6', 'Six', '11')
insert #ranks values('7', 'Seven', '13')
insert #ranks values('8', 'Eight', '17')
insert #ranks values('9', 'Nine', '19')
insert #ranks values('T', 'Ten', '23')
insert #ranks values('J', 'Jack', '29')
insert #ranks values('Q', 'Queen', '31')
insert #ranks values('K', 'King', '37')
insert #ranks values('A', 'Ace', '41')

IF object_id('tempdb..#suits') IS NOT NULL DROP TABLE #suits

create table #suits (
	suit_code char(1),
	suit_name char(10)
)

insert #suits values('s', 'Spades')
insert #suits values('h', 'Hearts')
insert #suits values('c', 'Clubs')
insert #suits values('d', 'Diamonds')

IF object_id('tempdb..#cards') IS NOT NULL DROP TABLE #cards

select c.*, s.*, c.rank_name + ' of ' + s.suit_name as card_text, c.rank_code + s.suit_code as card_code
into #cards
from #suits s
cross join #ranks c

select * from #cards order by suit_code desc, rank_prime

-------------------------------------------------------------------------------------------
-- (2) Set up relative rankings of combinations of 1 card (simplest case, i.e. simple card value wins)

IF object_id('tempdb..#combos1') IS NOT NULL DROP TABLE #combos1

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos1
FROM (
	SELECT DISTINCT 
		rank_prime AS hand_signature, 
		rank_name AS hand_name, 
		rank_prime AS hand_value 
	FROM #cards
) d

SELECT * FROM #combos1 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos1 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (3) Set up relative rankings of combinations of 2 cards (pairs or highs only)

IF object_id('tempdb..#combos2') IS NOT NULL DROP TABLE #combos2

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos2
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name + 's'
			ELSE c1.rank_name +' High with '+c2.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime THEN 200000 + ( c1.rank_prime * 100 )
			ELSE 100000 + ( c1.rank_prime * 100 ) + ( c2.rank_prime )
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	WHERE c1.card_code <> c2.card_code -- don't allow a card to be combined with itself
	AND c1.rank_prime >= c2.rank_prime -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos2 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos2 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (4) Set up relative rankings of combinations of 3 cards (three of a kind, pairs or highs only)

IF object_id('tempdb..#combos3') IS NOT NULL DROP TABLE #combos3

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos3
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime * c3.rank_prime  AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 'Three ' + c1.rank_name + 's'
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name + 's with ' +c3.rank_name
			WHEN c2.rank_prime = c3.rank_prime THEN 'Pair of ' + c2.rank_name + 's with ' +c1.rank_name
			ELSE c1.rank_name +' High with '+c2.rank_name+', '+c3.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 30000000 + ( c1.rank_prime * 10000 )
			WHEN c1.rank_prime = c2.rank_prime THEN 20000000 + ( c1.rank_prime * 10000 ) + ( c3.rank_prime ) 
			WHEN c2.rank_prime = c3.rank_prime THEN 20000000 + ( c2.rank_prime * 10000 ) + ( c1.rank_prime ) 
			ELSE 10000000 + ( c1.rank_prime * 10000 ) + ( c2.rank_prime * 100) + ( c3.rank_prime ) 
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	CROSS JOIN #cards c3
	WHERE ( c1.card_code <> c2.card_code AND c1.card_code <> c3.card_code AND c2.card_code <> c3.card_code ) -- don't allow a card to be combined with itself
	AND ( c1.rank_prime >= c2.rank_prime AND c2.rank_prime >= c3.rank_prime ) -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos3 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos3 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));


-------------------------------------------------------------------------------------------
-- (4) Set up relative rankings of combinations of 4 cards (four of a kind, three of a kind, two pairs, pairs or highs only)

IF object_id('tempdb..#combos4') IS NOT NULL DROP TABLE #combos4

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos4
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime * c3.rank_prime * c4.rank_prime  AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Four ' + c1.rank_name + 's'
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 'Three ' + c1.rank_name + 's'
			WHEN c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Three ' + c2.rank_name + 's'
			WHEN c1.rank_prime = c2.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Two Pair, ' + c1.rank_name + 's over ' + c3.rank_name + 's'
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name + 's with ' + c3.rank_name + ', ' + c4.rank_name
			WHEN c2.rank_prime = c3.rank_prime THEN 'Pair of ' + c2.rank_name + 's with ' + c1.rank_name + ', ' + c4.rank_name
			WHEN c3.rank_prime = c4.rank_prime THEN 'Pair of ' + c3.rank_name + 's with ' + c1.rank_name + ', ' + c2.rank_name
			ELSE c1.rank_name +' High with '+c2.rank_name+', '+c3.rank_name+', '+c4.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 5000000000 + ( c1.rank_prime * 1000000 ) -- 4
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 4000000000 + ( c1.rank_prime * 1000000 ) -- 3
			WHEN c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 4000000000 + ( c2.rank_prime * 1000000 ) -- 3
			WHEN c1.rank_prime = c2.rank_prime AND c3.rank_prime = c4.rank_prime THEN 3000000000 + ( c1.rank_prime * 1000000 ) + ( c3.rank_prime * 10000 ) -- 2 pair
			WHEN c1.rank_prime = c2.rank_prime THEN 2000000000 + ( c1.rank_prime * 1000000 )+ ( c3.rank_prime * 10000 ) + ( c4.rank_prime * 100 ) 
			WHEN c2.rank_prime = c3.rank_prime THEN 2000000000 + ( c2.rank_prime * 1000000 )+ ( c1.rank_prime * 10000 ) + ( c4.rank_prime * 100) 
			WHEN c3.rank_prime = c4.rank_prime THEN 2000000000 + ( c2.rank_prime * 1000000 )+ ( c1.rank_prime * 10000 ) + ( c2.rank_prime * 100) 
			ELSE 1000000000 + ( c1.rank_prime * 1000000 ) + ( c2.rank_prime * 10000) + ( c3.rank_prime * 100) + ( c4.rank_prime ) 
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	CROSS JOIN #cards c3
	CROSS JOIN #cards c4
	WHERE ( 
		c1.card_code <> c2.card_code AND
		c1.card_code <> c3.card_code AND 
		c1.card_code <> c4.card_code AND 
		c2.card_code <> c3.card_code AND
		c2.card_code <> c4.card_code AND
		c3.card_code <> c4.card_code 
	) -- don't allow a card to be combined with itself
	AND ( 
		c1.rank_prime >= c2.rank_prime AND
		c2.rank_prime >= c3.rank_prime AND
		c3.rank_prime >= c4.rank_prime 
	) -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos4 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos4 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));



-------------------------------------------------------------------------------------------
-- (5) Extract combinations of 5 from 6

IF object_id('tempdb..#card_positions') IS NOT NULL DROP TABLE #card_positions

create table #card_positions (
	p int
)

insert #card_positions values(1)
insert #card_positions values(2)
insert #card_positions values(3)
insert #card_positions values(4)
insert #card_positions values(5)
insert #card_positions values(6)


select p1.p as cp1, p2.p as cp2, p3.p as cp3, p4.p as cp4, p5.p as cp5,
	'{'+LTRIM(STR(p1.p))+', '+LTRIM(STR(p2.p))+', '+LTRIM(STR(p3.p))+', '+LTRIM(STR(p4.p))+', '+LTRIM(STR(p5.p))+'}' as List
from #card_positions p1
CROSS JOIN #card_positions p2
CROSS JOIN #card_positions p3
CROSS JOIN #card_positions p4
CROSS JOIN #card_positions p5
WHERE ( 
	p1.p <> p2.p AND p1.p <> p3.p AND p1.p <> p4.p AND p1.p <> p5.p
		         AND p2.p <> p3.p AND p2.p <> p4.p AND p2.p <> p5.p
		                          AND p3.p <> p4.p AND p3.p <> p5.p
		                                           AND p4.p <> p5.p
) -- don't allow a position to be combined with itself
AND ( 
	p1.p < p2.p AND
	p2.p < p3.p AND
	p3.p < p4.p AND
	p4.p < p5.p 
) -- consider only the combinations where the positions are in ascending order from left to right

-------------------------------------------------------------------------------------------
-- (6) Extract combinations of 5 from 7

IF object_id('tempdb..#card_positions') IS NOT NULL DROP TABLE #card_positions

create table #card_positions (
	p int
)

insert #card_positions values(1)
insert #card_positions values(2)
insert #card_positions values(3)
insert #card_positions values(4)
insert #card_positions values(5)
insert #card_positions values(6)
insert #card_positions values(7)

select p1.p as cp1, p2.p as cp2, p3.p as cp3, p4.p as cp4, p5.p as cp5,
	'{'+LTRIM(STR(p1.p))+', '+LTRIM(STR(p2.p))+', '+LTRIM(STR(p3.p))+', '+LTRIM(STR(p4.p))+', '+LTRIM(STR(p5.p))+'}' as List
from #card_positions p1
CROSS JOIN #card_positions p2
CROSS JOIN #card_positions p3
CROSS JOIN #card_positions p4
CROSS JOIN #card_positions p5
WHERE ( 
	p1.p <> p2.p AND p1.p <> p3.p AND p1.p <> p4.p AND p1.p <> p5.p
		         AND p2.p <> p3.p AND p2.p <> p4.p AND p2.p <> p5.p
		                          AND p3.p <> p4.p AND p3.p <> p5.p
		                                           AND p4.p <> p5.p
) -- don't allow a position to be combined with itself
AND ( 
	p1.p < p2.p AND
	p2.p < p3.p AND
	p3.p < p4.p AND
	p4.p < p5.p 
) -- consider only the combinations where the positions are in ascending order from left to right

