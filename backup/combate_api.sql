-- MySQL dump 10.13  Distrib 26.7.0, for Win64 (x86_64)
--
-- Host: localhost    Database: combate_api
-- ------------------------------------------------------
-- Server version	26.7.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `combate_api`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `combate_api` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `combate_api`;

--
-- Table structure for table `combates`
--

DROP TABLE IF EXISTS `combates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `combates` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Personaje1Id` int NOT NULL,
  `Personaje2Id` int NOT NULL,
  `VidaP1` int NOT NULL,
  `VidaP2` int NOT NULL,
  `RecursoP1` int NOT NULL,
  `RecursoP2` int NOT NULL,
  `Estado` enum('EnCurso','Finalizado') NOT NULL DEFAULT 'EnCurso',
  `GanadorId` int DEFAULT NULL,
  `Fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `FK_Combates_P1` (`Personaje1Id`),
  KEY `FK_Combates_P2` (`Personaje2Id`),
  KEY `FK_Combates_Ganador` (`GanadorId`),
  CONSTRAINT `FK_Combates_Ganador` FOREIGN KEY (`GanadorId`) REFERENCES `personajes` (`Id`),
  CONSTRAINT `FK_Combates_P1` FOREIGN KEY (`Personaje1Id`) REFERENCES `personajes` (`Id`),
  CONSTRAINT `FK_Combates_P2` FOREIGN KEY (`Personaje2Id`) REFERENCES `personajes` (`Id`),
  CONSTRAINT `CHK_Combates_Distintos` CHECK ((`Personaje1Id` <> `Personaje2Id`)),
  CONSTRAINT `CHK_Combates_Recursos` CHECK (((`RecursoP1` >= 0) and (`RecursoP2` >= 0))),
  CONSTRAINT `CHK_Combates_Vidas` CHECK (((`VidaP1` >= 0) and (`VidaP2` >= 0)))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `combates`
--

LOCK TABLES `combates` WRITE;
/*!40000 ALTER TABLE `combates` DISABLE KEYS */;
INSERT INTO `combates` VALUES (1,1,2,100,64,10,30,'EnCurso',NULL,'2026-11-16 05:18:45'),(2,3,4,100,90,10,30,'EnCurso',NULL,'2026-11-16 05:18:45'),(3,4,2,67,0,0,30,'Finalizado',4,'2026-11-16 05:18:45');
/*!40000 ALTER TABLE `combates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personajes`
--

DROP TABLE IF EXISTS `personajes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personajes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  `Tipo` varchar(20) NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `CHK_Personajes_Tipo` CHECK ((`Tipo` in ('Guerrero','Mago','Arquero','Asesino')))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personajes`
--

LOCK TABLES `personajes` WRITE;
/*!40000 ALTER TABLE `personajes` DISABLE KEYS */;
INSERT INTO `personajes` VALUES (1,'Thorin','Guerrero'),(2,'Gandalf','Mago'),(3,'Legolas','Arquero'),(4,'Erebus','Asesino'),(5,'Conan','Guerrero');
/*!40000 ALTER TABLE `personajes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `turnos`
--

DROP TABLE IF EXISTS `turnos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `turnos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CombateId` int NOT NULL,
  `Numero` int NOT NULL,
  `AtacanteId` int NOT NULL,
  `DefensorId` int NOT NULL,
  `Accion` enum('atacar','habilidad') NOT NULL,
  `Danio` int NOT NULL,
  `Nota` varchar(200) DEFAULT NULL,
  `VidaP1Antes` int NOT NULL,
  `VidaP2Antes` int NOT NULL,
  `RecursoP1Antes` int NOT NULL,
  `RecursoP2Antes` int NOT NULL,
  `Fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UQ_Turnos_CombateNumero` (`CombateId`,`Numero`),
  KEY `FK_Turnos_Atacante` (`AtacanteId`),
  KEY `FK_Turnos_Defensor` (`DefensorId`),
  CONSTRAINT `FK_Turnos_Atacante` FOREIGN KEY (`AtacanteId`) REFERENCES `personajes` (`Id`),
  CONSTRAINT `FK_Turnos_Combate` FOREIGN KEY (`CombateId`) REFERENCES `combates` (`Id`),
  CONSTRAINT `FK_Turnos_Defensor` FOREIGN KEY (`DefensorId`) REFERENCES `personajes` (`Id`),
  CONSTRAINT `CHK_Turnos_Danio` CHECK ((`Danio` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `turnos`
--

LOCK TABLES `turnos` WRITE;
/*!40000 ALTER TABLE `turnos` DISABLE KEYS */;
INSERT INTO `turnos` VALUES (1,1,1,1,2,'atacar',16,'Primer golpe de Thorin',120,80,0,40,'2026-11-16 05:18:45'),(2,1,2,2,1,'habilidad',20,NULL,120,64,10,40,'2026-11-16 05:18:45'),(3,3,1,4,2,'atacar',41,NULL,90,80,30,40,'2026-11-16 05:18:45'),(4,3,2,2,4,'habilidad',23,NULL,90,39,15,40,'2026-11-16 05:18:45'),(5,3,3,4,2,'atacar',39,'Golpe final',67,39,15,30,'2026-11-16 05:18:45');
/*!40000 ALTER TABLE `turnos` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed
