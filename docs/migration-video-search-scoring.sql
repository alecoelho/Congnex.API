-- Migration: Add video search scoring/ranking metadata to lesson_videos
-- Date: 2024
-- Description: Adds columns to store match score, confidence, matched structures, and search source

ALTER TABLE lesson_videos ADD COLUMN match_score INT NULL;
ALTER TABLE lesson_videos ADD COLUMN match_confidence VARCHAR(20) NULL;
ALTER TABLE lesson_videos ADD COLUMN matched_structures JSON NULL;
ALTER TABLE lesson_videos ADD COLUMN search_source VARCHAR(50) NULL;
