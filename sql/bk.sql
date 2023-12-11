--
-- PostgreSQL database dumpa
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
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.results OWNER TO elder123;

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
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.products (id, name, enable) FROM stdin;
3	TRIPLE CARACAS	f
1	LA GRANJITA	f
4	PAL CASERITO	f
2	TRIPLE ZAMORANO	t
5	LOTTO REY	t
\.


--
-- Data for Name: provider_product; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.provider_product (id, provider_id, product_id, cron_expression, slug) FROM stdin;
1	1	2	5,10,20 12,16,19 * * *	TRIPLE ZAMORANO
3	4	5	35,45,55 8,9,10,11,12,13,14,15,16,17,18,19 * * *	\N
\.


--
-- Data for Name: providers; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.providers (id, url, enable, name) FROM stdin;
2	https://loteriadehoy.com/loteria/triplezamorano/resultados	f	Triple Zamonaro Loteria de hoy
1	http://triplezamorano.com/action/index	t	Triple Zamonaro Oficial
4	https://lottorey.com.ve	f	Lotto Rey Oficial
\.


--
-- Data for Name: results; Type: TABLE DATA; Schema: public; Owner: elder123
--

COPY public.results (id, result, date, "time", product_id, provider_id, created_at) FROM stdin;
6	714		12:00 PM	2	1	2023-12-06 12:10:07.802-04
12	078		07:00 PM	2	1	2023-12-07 19:10:04.158572-04
13	964		04:00 PM	2	1	2023-12-07 19:10:04.158572-04
14	716		12:00 PM	2	1	2023-12-07 19:10:04.158572-04
85	10 Tigre		11:30 am	5	4	2023-12-11 11:55:20.96017-04
86	36 Culebra		09:30 am	5	4	2023-12-11 11:55:20.96017-04
20	659		07:00 PM	2	1	2023-12-08 19:10:13.033191-04
21	803		04:00 PM	2	1	2023-12-08 19:10:13.033191-04
22	501		12:00 PM	2	1	2023-12-08 19:10:13.033191-04
87	35 Jirafa		08:30 am	5	4	2023-12-11 11:55:20.96017-04
32	096		07:00 PM	2	1	2023-12-09 19:20:05.685159-04
33	926		04:00 PM	2	1	2023-12-09 19:20:05.685159-04
34	245		12:00 PM	2	1	2023-12-09 19:20:05.685159-04
37	853		07:00 PM	2	1	2023-12-10 19:20:05.729573-04
\.


--
-- Name: product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.product_id_seq', 5, true);


--
-- Name: providers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.providers_id_seq', 4, true);


--
-- Name: providers_products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.providers_products_id_seq', 3, true);


--
-- Name: results_id_seq; Type: SEQUENCE SET; Schema: public; Owner: elder123
--

SELECT pg_catalog.setval('public.results_id_seq', 87, true);


--
-- Name: products product_pk; Type: CONSTRAINT; Schema: public; Owner: elder123
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT product_pk PRIMARY KEY (id);


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
-- PostgreSQL database dump complete
--

