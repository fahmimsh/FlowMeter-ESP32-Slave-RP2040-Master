-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Jun 09, 2024 at 05:20 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `flowmeter_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `log_data_1_2`
--

CREATE TABLE `log_data_1_2` (
  `id` int(32) UNSIGNED NOT NULL,
  `flow_meter` varchar(32) NOT NULL,
  `mode` varchar(8) NOT NULL,
  `set_liter` double NOT NULL,
  `liter` double NOT NULL,
  `k_factor` double NOT NULL,
  `batch` varchar(12) NOT NULL,
  `transfer_to` varchar(12) NOT NULL,
  `date_time` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `log_data_3_4_5`
--

CREATE TABLE `log_data_3_4_5` (
  `id` int(32) UNSIGNED NOT NULL,
  `proses_mesin` varchar(8) NOT NULL,
  `transfer_to` varchar(8) NOT NULL,
  `batch` varchar(8) NOT NULL,
  `produk` varchar(16) DEFAULT NULL,
  `liter` double NOT NULL,
  `k_factor` double NOT NULL,
  `date_time` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `log_data_1_2`
--
ALTER TABLE `log_data_1_2`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `log_data_3_4_5`
--
ALTER TABLE `log_data_3_4_5`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `log_data_1_2`
--
ALTER TABLE `log_data_1_2`
  MODIFY `id` int(32) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `log_data_3_4_5`
--
ALTER TABLE `log_data_3_4_5`
  MODIFY `id` int(32) UNSIGNED NOT NULL AUTO_INCREMENT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
