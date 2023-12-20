--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1 (Debian 16.1-1.pgdg120+1)
-- Dumped by pg_dump version 16.1 (Debian 16.1-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: products; Type: TABLE; Schema: public; Owner: elder123
--

CREATE TABLE public.products (
    id integer NOT NULL,
    name character varying NOT NULL,
    enable boolean DEFAULT false NOT NULL
);


ALTER TABLE public.products OWNER TO elder123;

--
-- Name: product_id_seq; Type: SEQUENCE; Schema: public; Owner: elder123
--

CREATE SEQUENCE public.product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.product_id_seq OWNER TO elder123;

--
-- Name: product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: elder123
--

ALTER SEQUENCE public.product_id_seq OWNED BY public.products.id;


--
-- Name: product_type; Type: TABLE; Schema: public; Owner: elder123
--

CREATE TABLE public.product_type (
    id integer NOT NULL,
    name character varying NOT NULL
);


ALTER TABLE public.product_type OWNER TO elder123;

--
-- Name: product_type_id_seq; Type: SEQUENCE; Schema: public; Owner: elder123
--

CREATE SEQUENCE public.product_type_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.product_type_id_seq OWNER TO elder123;

--
-- Name: product_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: elder123
--

ALTER SEQUENCE public.product_type_id_seq OWNED BY public.product_type.id;


--
-- Name: provider_product; Type: TABLE; Schema: public; Owner: elder123
--

CREATE TABLE public.provider_product (
    id integer NOT NULL,
    provider_id integer NOT NULL,
    product_id integer NOT NULL,
    cron_expression character varying,
    slug character varying
);


ALTER TABLE public.provider_product OWNER TO elder123;

--
-- Name: providers; Type: TABLE; Schema: public; Owner: elder123
--

CREATE TABLE public.providers (
    id integer NOT NULL,
    url character varying NOT NULL,
    enable boolean DEFAULT false NOT NULL,
    name character varying
);


ALTER TABLE public.providers OWNER TO elder123;

--
-- Name: providers_id_seq; Type: SEQUENCE; Schema: public; Owner: elder123
--

CREATE SEQUENCE public.providers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.providers_id_seq OWNER TO elder123;

--
-- Name: providers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: elder123
--

ALTER SEQUENCE public.providers_id_seq OWNED BY public.providers.id;


--
-- Name: providers_products_id_seq; Type: SEQUENCE; Schema: public; Owner: elder123
--

CREATE SEQUENCE public.providers_products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.providers_products_id_seq OWNER TO elder123;

--
-- Name: providers_products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: elder123
--

ALTER SEQUENCE public.providers_products_id_seq OWNED BY public.provider_product.id;


--
-- Name: results; Type: TABLE; Schema: public; Owner: elder123
--

CREATE TABLE public.results (
    id integer NOT NULL,
    result character varying NOT NULL,
    date character varying NOT NULL,
    "time" character varying NOT NULL,
    product_id integer NOT NULL,
    provider_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    sorteo character varying,
    product_type_id integer
);


ALTER TABLE public.results OWNER TO elder123;

--
-- Name: COLUMN results.sorteo; Type: COMMENT; Schema: public; Owner: elder123
--

COMMENT ON COLUMN public.results.sorteo IS 'se almacena el nombre del sorteo en caso de que el poroducto tenga mas de un sorteo por hora, ejemplo tripla A y triple B a las 7:00';


--
-- Name: results_id_seq; Type: SEQUENCE; Schema: public; Owner: elder123
--

CREATE SEQUENCE public.results_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.results_id_seq OWNER TO elder123;

--
-- Name: results_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: elder123
--

ALTER SEQUENCE public.results_id_seq OWNED BY public.results.id;


--
-- Name: product_type id; Type: DEFAULT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.product_type ALTER COLUMN id SET DEFAULT nextval('public.product_type_id_seq'::regclass);


--
-- Name: products id; Type: DEFAULT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.products ALTER COLUMN id SET DEFAULT nextval('public.product_id_seq'::regclass);


--
-- Name: provider_product id; Type: DEFAULT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.provider_product ALTER COLUMN id SET DEFAULT nextval('public.providers_products_id_seq'::regclass);


--
-- Name: providers id; Type: DEFAULT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.providers ALTER COLUMN id SET DEFAULT nextval('public.providers_id_seq'::regclass);


--
-- Name: results id; Type: DEFAULT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.results ALTER COLUMN id SET DEFAULT nextval('public.results_id_seq'::regclass);


--
-- Data for Name: product_type; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.product_type (id, name) FROM stdin;
1	ANIMALITOS
2	TRIPLES
3	TERMINALES
4	TRIPLETA
5	ANIMALES77
6	PEGA5
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.products (id, name, enable) FROM stdin;
1	LA GRANJITA	f
2	TRIPLE ZAMORANO	t
5	LOTTO REY	t
4	TRIPLE CALIENTE	t
7	RULETA ACTIVA	f
8	EL RUCO	t
9	LA RUCA	t
6	TRIPLE ZULIA	t
3	TRIPLE CARACAS	t
10	SELVA PLUS	t
11	GUACHARO ACTIVO	t
\.


--
-- Data for Name: provider_product; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.provider_product (id, provider_id, product_id, cron_expression, slug) FROM stdin;
1	1	2	5,10,20 12,16,19 * * *	TRIPLE ZAMORANO
4	5	6	10,15,20,50,55,59 12,16,19 * * *	\N
3	4	5	35,45,55 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
5	6	4	5,10,15,20,25,35,40,45 12,16,19 * * *	\N
6	7	8	5,10,15 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
7	8	9	20,25,30 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
8	9	3	5,10,15,35,40,45 13,16,19 * * *	\N
9	10	10	5,10,15 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
10	11	11	5,10,15 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
\.


--
-- Data for Name: providers; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.providers (id, url, enable, name) FROM stdin;
2	https://loteriadehoy.com/loteria/triplezamorano/resultados	f	Triple Zamonaro Loteria de hoy
1	http://triplezamorano.com/action/index	t	Triple Zamonaro Oficial
4	https://lottorey.com.ve	f	Lotto Rey Oficial
5	http://www.resultadostriplezulia.com/action/index	f	Triple Zulia Oficial
6	http://www.triplecaliente.com/action/index	f	Triple Caliente Oficial
7	https://lottollano.com.ve/elruco.php	f	El Ruco Oficial
8	https://lottollano.com.ve/laruca.php	f	La Ruca Oficial
9	http://165.227.185.69:8000/api/resultados	f	Triple Caracas Oficial
10	https://guacharoactivo.com/#/resultados-selva-plus	f	SelvaPlus Oficial
11	https://guacharoactivo.com/#/resultados-guacharo	f	Guacharo Activo Oficial
\.


--
-- Data for Name: results; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.results (id, result, date, "time", product_id, provider_id, created_at, sorteo, product_type_id) FROM stdin;
1425	21 Gallo		08:00 AM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1426	20 Cochino		09:00 AM	10	10	2023-12-19 16:15:08.731236-04	\N	1
408	27 Perro		09:30 am	5	4	2023-12-12 19:55:10.077975-04	\N	1
531	329		04:00 PM	2	1	2023-12-14 16:20:11.991769-04	\N	2
532	269		12:00 PM	2	1	2023-12-14 16:20:11.991769-04	\N	2
702	862		07:00 PM	2	1	2023-12-15 19:20:14.037094-04	\N	2
703	173		04:00 PM	2	1	2023-12-15 19:20:14.037094-04	\N	2
704	324		12:00 PM	2	1	2023-12-15 19:20:14.037094-04	\N	2
161	25 Gallina		04:30 pm	5	4	2023-12-11 16:55:10.993314-04	\N	1
162	34 Venado		03:30 pm	5	4	2023-12-11 16:55:10.993314-04	\N	1
163	13 Mono		02:30 pm	5	4	2023-12-11 16:55:10.993314-04	\N	1
164	29 Elefante		01:30 pm	5	4	2023-12-11 16:55:10.993314-04	\N	1
165	16 Oso		12:30 pm	5	4	2023-12-11 16:55:10.993314-04	\N	1
166	10 Tigre		11:30 am	5	4	2023-12-11 16:55:10.993314-04	\N	1
167	36 Culebra		09:30 am	5	4	2023-12-11 16:55:10.993314-04	\N	1
168	35 Jirafa		08:30 am	5	4	2023-12-11 16:55:10.993314-04	\N	1
6	714		12:00 PM	2	1	2023-12-06 12:10:07.802-04	\N	2
144	207		12:00 PM	2	1	2023-12-11 16:20:19.306931-04	\N	2
705	827		07:10 PM	4	6	2023-12-15 19:20:15.174065-04	Triple A	2
706	594		07:10 PM	4	6	2023-12-15 19:20:15.174065-04	Triple B	2
707	701		04:30 PM	4	6	2023-12-15 19:20:15.174065-04	Triple A	2
708	961		04:30 PM	4	6	2023-12-15 19:20:15.174065-04	Triple B	2
709	503		01:00 PM	4	6	2023-12-15 19:20:15.174065-04	Triple A	2
710	710		01:00 PM	4	6	2023-12-15 19:20:15.174065-04	Triple B	2
458	522		04:00 PM	2	1	2023-12-13 16:20:06.936871-04	\N	2
459	184		12:00 PM	2	1	2023-12-13 16:20:06.936871-04	\N	2
464	26 Vaca		04:30 pm	5	4	2023-12-13 16:35:09.13266-04	\N	1
465	0 Delfín		03:30 pm	5	4	2023-12-13 16:35:09.13266-04	\N	1
466	18 Burro		02:30 pm	5	4	2023-12-13 16:35:09.13266-04	\N	1
467	19 Chivo		01:30 pm	5	4	2023-12-13 16:35:09.13266-04	\N	1
468	05 León		12:30 pm	5	4	2023-12-13 16:35:09.13266-04	\N	1
469	25 Gallina		11:30 am	5	4	2023-12-13 16:35:09.13266-04	\N	1
470	29 Elefante		10:30 am	5	4	2023-12-13 16:35:09.13266-04	\N	1
471	10 Tigre		09:30 am	5	4	2023-12-13 16:35:09.13266-04	\N	1
472	22 Camello		08:30 am	5	4	2023-12-13 16:35:09.13266-04	\N	1
398	23 Zebra		07:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
399	31 Lapa		06:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
400	04 Alacrán		05:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
401	15 Zorro		04:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
402	01 Carnero		03:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
1008	437		01:00 PM	4	6	2023-12-18 16:15:12.125068-04	Triple A	2
1009	702		01:00 PM	4	6	2023-12-18 16:15:12.125068-04	Triple B	2
817	942		07:00 PM	2	1	2023-12-17 19:20:35.378914-04	\N	2
403	11 Gato		02:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
404	24 Iguana		01:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
405	21 Gallo		12:30 pm	5	4	2023-12-12 19:55:10.077975-04	\N	1
406	35 Jirafa		11:30 am	5	4	2023-12-12 19:55:10.077975-04	\N	1
407	30 Caimán		10:30 am	5	4	2023-12-12 19:55:10.077975-04	\N	1
12	078		07:00 PM	2	1	2023-12-07 19:10:04.158572-04	\N	2
13	964		04:00 PM	2	1	2023-12-07 19:10:04.158572-04	\N	2
14	716		12:00 PM	2	1	2023-12-07 19:10:04.158572-04	\N	2
20	659		07:00 PM	2	1	2023-12-08 19:10:13.033191-04	\N	2
21	803		04:00 PM	2	1	2023-12-08 19:10:13.033191-04	\N	2
22	501		12:00 PM	2	1	2023-12-08 19:10:13.033191-04	\N	2
32	096		07:00 PM	2	1	2023-12-09 19:20:05.685159-04	\N	2
33	926		04:00 PM	2	1	2023-12-09 19:20:05.685159-04	\N	2
34	245		12:00 PM	2	1	2023-12-09 19:20:05.685159-04	\N	2
37	853		07:00 PM	2	1	2023-12-10 19:20:05.729573-04	\N	2
373	924		07:00 PM	2	1	2023-12-12 19:20:06.144739-04	\N	2
374	195		04:00 PM	2	1	2023-12-12 19:20:06.144739-04	\N	2
375	905		12:00 PM	2	1	2023-12-12 19:20:06.144739-04	\N	2
1010	140		12:45 PM	6	5	2023-12-18 16:15:13.021711-04	Triple A	2
1011	057		12:45 PM	6	5	2023-12-18 16:15:13.021711-04	Triple B	2
1012	06		11:00 AM	8	7	2023-12-18 16:15:36.073052-04	\N	3
1013	57		10:00 AM	8	7	2023-12-18 16:15:36.073052-04	\N	3
839	55		07:15 PM	9	8	2023-12-17 19:30:13.715276-04	\N	3
1014	69		12:00 PM	8	7	2023-12-18 16:15:36.073052-04	\N	3
1015	270		13:00:00	3	8	2023-12-18 19:04:08.665659-04	Triple A	2
1016	563		16:30:00	3	8	2023-12-18 19:04:08.665659-04	Triple A	2
573	03 Ciempies		04:30 pm	5	4	2023-12-14 16:55:16.551445-04	\N	1
574	31 Lapa		03:30 pm	5	4	2023-12-14 16:55:16.551445-04	\N	1
575	24 Iguana		02:30 pm	5	4	2023-12-14 16:55:16.551445-04	\N	1
576	10 Tigre		01:30 pm	5	4	2023-12-14 16:55:16.551445-04	\N	1
577	35 Jirafa		12:30 pm	5	4	2023-12-14 16:55:16.551445-04	\N	1
578	04 Alacrán		11:30 am	5	4	2023-12-14 16:55:16.551445-04	\N	1
579	09 Águila		10:30 am	5	4	2023-12-14 16:55:16.551445-04	\N	1
580	34 Venado		09:30 am	5	4	2023-12-14 16:55:16.551445-04	\N	1
581	08 Ratón		08:30 am	5	4	2023-12-14 16:55:16.551445-04	\N	1
855	728		07:10 PM	4	6	2023-12-17 19:45:11.995964-04	Triple A	2
856	303		07:10 PM	4	6	2023-12-17 19:45:11.995964-04	Triple B	2
477	684		12:45 PM	6	5	2023-12-13 16:36:52.232645-04	Triple A	2
478	729		12:45 PM	6	5	2023-12-13 16:36:52.232645-04	Triple B	2
569	958		04:45 PM	6	5	2023-12-14 16:55:15.912282-04	Triple A	2
570	708		04:45 PM	6	5	2023-12-14 16:55:15.912282-04	Triple B	2
571	735		12:45 PM	6	5	2023-12-14 16:55:15.912282-04	Triple A	2
572	093		12:45 PM	6	5	2023-12-14 16:55:15.912282-04	Triple B	2
475	156		01:00 PM	4	6	2023-12-13 16:36:43.508996-04	Triple A	2
476	710		01:00 PM	4	6	2023-12-13 16:36:43.508996-04	Triple B	2
552	926		04:30 PM	4	6	2023-12-14 16:45:15.425111-04	Triple A	2
553	606		04:30 PM	4	6	2023-12-14 16:45:15.425111-04	Triple B	2
554	356		01:00 PM	4	6	2023-12-14 16:45:15.425111-04	Triple A	2
555	204		01:00 PM	4	6	2023-12-14 16:45:15.425111-04	Triple B	2
696	900		07:05 PM	6	5	2023-12-15 19:15:12.815014-04	Triple A	2
697	176		07:05 PM	6	5	2023-12-15 19:15:12.815014-04	Triple B	2
698	893		04:45 PM	6	5	2023-12-15 19:15:12.815014-04	Triple A	2
699	508		04:45 PM	6	5	2023-12-15 19:15:12.815014-04	Triple B	2
700	149		12:45 PM	6	5	2023-12-15 19:15:12.815014-04	Triple A	2
701	006		12:45 PM	6	5	2023-12-15 19:15:12.815014-04	Triple B	2
783	130		01:00 PM	4	6	2023-12-16 12:37:26.039042-04	Triple A	2
784	119		01:00 PM	4	6	2023-12-16 12:37:26.039042-04	Triple B	2
785	014		12:45 PM	6	5	2023-12-16 12:37:26.271332-04	Triple A	2
786	929		12:45 PM	6	5	2023-12-16 12:37:26.271332-04	Triple B	2
787	25 Gallina		01:30 pm	5	4	2023-12-16 12:37:27.929342-04	\N	1
788	01 Carnero		12:30 pm	5	4	2023-12-16 12:37:27.929342-04	\N	1
789	23 Zebra		11:30 am	5	4	2023-12-16 12:37:27.929342-04	\N	1
790	32 Ardilla		10:30 am	5	4	2023-12-16 12:37:27.929342-04	\N	1
791	06 Rana		09:30 am	5	4	2023-12-16 12:37:27.929342-04	\N	1
792	12 Caballo		08:30 am	5	4	2023-12-16 12:37:27.929342-04	\N	1
1427	28 Zamuro		10:00 AM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1428	10 Tigre		11:00 AM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1429	13 Mono		12:00 PM	10	10	2023-12-19 16:15:08.731236-04	\N	1
659	21 Gallo		06:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
660	26 Vaca		05:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
661	33 Pescado		04:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
662	10 Tigre		03:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
663	29 Elefante		02:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
664	19 Chivo		01:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
665	24 Iguana		12:30 pm	5	4	2023-12-15 18:45:41.640731-04	\N	1
666	17 Pavo		11:30 am	5	4	2023-12-15 18:45:41.640731-04	\N	1
667	11 Gato		10:30 am	5	4	2023-12-15 18:45:41.640731-04	\N	1
668	18 Burro		09:30 am	5	4	2023-12-15 18:45:41.640731-04	\N	1
669	32 Ardilla		08:30 am	5	4	2023-12-15 18:45:41.640731-04	\N	1
1430	22 Camello		01:00 PM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1431	15 Zorro		02:00 PM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1432	6 Rana		03:00 PM	10	10	2023-12-19 16:15:08.731236-04	\N	1
799	16		09:00 AM	8	7	2023-12-16 12:37:35.957623-04	\N	3
800	58		10:00 AM	8	7	2023-12-16 12:37:35.957623-04	\N	3
801	66		11:00 AM	8	7	2023-12-16 12:37:35.957623-04	\N	3
802	19		12:00 PM	8	7	2023-12-16 12:37:35.957623-04	\N	3
803	88		01:00 PM	8	7	2023-12-16 12:37:35.957623-04	\N	3
804	03		02:00 PM	8	7	2023-12-16 12:37:35.957623-04	\N	3
811	68		09:15 AM	9	8	2023-12-16 14:30:12.987519-04	\N	3
812	05		10:15 AM	9	8	2023-12-16 14:30:12.987519-04	\N	3
813	25		11:15 AM	9	8	2023-12-16 14:30:12.987519-04	\N	3
814	30		12:15 PM	9	8	2023-12-16 14:30:12.987519-04	\N	3
815	18		01:15 PM	9	8	2023-12-16 14:30:12.987519-04	\N	3
816	39		02:15 PM	9	8	2023-12-16 14:30:12.987519-04	\N	3
1433	32 Ardilla		04:00 PM	10	10	2023-12-19 16:15:08.731236-04	\N	1
1457	086		01:00 PM	4	6	2023-12-19 16:20:10.957886-04	Triple A	2
1458	167		01:00 PM	4	6	2023-12-19 16:20:10.957886-04	Triple B	2
1459	906		12:45 PM	6	5	2023-12-19 16:20:11.213052-04	Triple A	2
1460	270		12:45 PM	6	5	2023-12-19 16:20:11.213052-04	Triple B	2
1463	48		09:15 AM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1000	649		12:00 PM	2	1	2023-12-18 16:10:10.818277-04	\N	2
1464	95		10:15 AM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1465	20		11:15 AM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1466	71		12:15 PM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1467	34		01:15 PM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1468	78		02:15 PM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1017	410		13:00:00	3	8	2023-12-18 19:04:08.665659-04	Triple B	2
1018	423		16:30:00	3	8	2023-12-18 19:04:08.665659-04	Triple B	2
1469	98		03:15 PM	9	8	2023-12-19 16:20:11.719154-04	\N	3
1471	16Oso		08:00 AM	11	11	2023-12-20 08:15:11.703416-04	\N	5
1472	32 Ardilla		08:00 AM	10	10	2023-12-20 08:15:11.794811-04	\N	1
1474	37		08:00 AM	8	7	2023-12-20 08:15:29.184458-04	\N	3
834	28		10:00 AM	8	7	2023-12-17 19:20:51.250426-04	\N	3
835	09		07:00 PM	8	7	2023-12-17 19:20:51.250426-04	\N	3
870	566		07:05 PM	6	5	2023-12-17 19:55:13.427809-04	Triple A	2
871	439		07:05 PM	6	5	2023-12-17 19:55:13.427809-04	Triple B	2
872	35 Jirafa		07:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
873	19 Chivo		06:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
874	11 Gato		05:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
875	21 Gallo		04:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
876	09 Águila		03:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
877	20 Cochino		02:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
878	27 Perro		01:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
879	13 Mono		12:30 pm	5	4	2023-12-17 19:55:14.70696-04	\N	1
880	01 Carnero		11:30 am	5	4	2023-12-17 19:55:14.70696-04	\N	1
881	06 Rana		10:30 am	5	4	2023-12-17 19:55:14.70696-04	\N	1
882	34 Venado		09:30 am	5	4	2023-12-17 19:55:14.70696-04	\N	1
971	29 Elefante		03:30 pm	5	4	2023-12-18 15:55:10.499971-04	\N	1
972	32 Ardilla		02:30 pm	5	4	2023-12-18 15:55:10.499971-04	\N	1
973	33 Pescado		01:30 pm	5	4	2023-12-18 15:55:10.499971-04	\N	1
974	25 Gallina		12:30 pm	5	4	2023-12-18 15:55:10.499971-04	\N	1
975	0 Delfín		11:30 am	5	4	2023-12-18 15:55:10.499971-04	\N	1
976	22 Camello		10:30 am	5	4	2023-12-18 15:55:10.499971-04	\N	1
977	28 Zamuro		09:30 am	5	4	2023-12-18 15:55:10.499971-04	\N	1
978	36 Culebra		08:30 am	5	4	2023-12-18 15:55:10.499971-04	\N	1
1434	68 Jaguar		08:00 AM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1435	44 Chiguire		09:00 AM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1436	58 Hormiga		10:00 AM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1437	45 Garza		11:00 AM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1438	60 Camaleon		12:00 PM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1439	32Ardilla		01:00 PM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1440	39 Lechuza		02:00 PM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1441	72 Gorila		03:00 PM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1442	46 Puma		04:00 PM	11	11	2023-12-19 16:15:08.822558-04	\N	5
1461	116		04:00 PM	2	1	2023-12-19 16:20:11.429194-04	\N	2
1462	513		12:00 PM	2	1	2023-12-19 16:20:11.429194-04	\N	2
1352	18 Burro		03:30 PM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1353	30 Caimán		02:30 PM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1354	20 Cochino		01:30 PM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1355	05 León		12:30 PM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1356	03 Ciempies		11:30 AM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1357	31 Lapa		10:30 AM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1358	06 Rana		09:30 AM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1359	12 Caballo		08:30 AM	5	4	2023-12-19 15:55:13.01483-04	\N	1
1443	178		01:00 PM	3	9	2023-12-19 16:15:08.835708-04	Triple A	2
1444	803		01:00 PM	3	9	2023-12-19 16:15:08.835708-04	Triple B	2
1447	53		08:00 AM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1448	18		09:00 AM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1449	26		10:00 AM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1450	60		11:00 AM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1451	08		12:00 PM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1452	22		01:00 PM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1453	91		02:00 PM	8	7	2023-12-19 16:15:10.567687-04	\N	3
1454	75		03:00 PM	8	7	2023-12-19 16:15:10.567687-04	\N	3
\.


--
-- Name: product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.product_id_seq', 11, true);


--
-- Name: product_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.product_type_id_seq', 6, true);


--
-- Name: providers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.providers_id_seq', 11, true);


--
-- Name: providers_products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.providers_products_id_seq', 10, true);


--
-- Name: results_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.results_id_seq', 1474, true);


--
-- Name: products product_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT product_pk PRIMARY KEY (id);


--
-- Name: product_type product_type_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.product_type
    ADD CONSTRAINT product_type_pk PRIMARY KEY (id);


--
-- Name: providers providers_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.providers
    ADD CONSTRAINT providers_pk PRIMARY KEY (id);


--
-- Name: provider_product providers_products_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.provider_product
    ADD CONSTRAINT providers_products_pk PRIMARY KEY (id);


--
-- Name: results results_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.results
    ADD CONSTRAINT results_pk PRIMARY KEY (id);


--
-- Name: provider_product provider_product_fk; Type: FK CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.provider_product
    ADD CONSTRAINT provider_product_fk FOREIGN KEY (provider_id) REFERENCES public.providers(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: provider_product provider_product_fk_1; Type: FK CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.provider_product
    ADD CONSTRAINT provider_product_fk_1 FOREIGN KEY (product_id) REFERENCES public.products(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: results results_fk; Type: FK CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.results
    ADD CONSTRAINT results_fk FOREIGN KEY (product_id) REFERENCES public.products(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: results results_fk_1; Type: FK CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.results
    ADD CONSTRAINT results_fk_1 FOREIGN KEY (provider_id) REFERENCES public.providers(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: results results_fk_2; Type: FK CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.results
    ADD CONSTRAINT results_fk_2 FOREIGN KEY (product_type_id) REFERENCES public.product_type(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

