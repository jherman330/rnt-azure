/**
 * Unit tests for StoryRootDisplay component
 * 
 * Note: These tests require test infrastructure setup (Jest, React Testing Library)
 * Run: npm test -- StoryRootDisplay.test.tsx
 */

import { render, screen } from '@testing-library/react';
import { StoryRootDisplay } from '../StoryRootDisplay';
import { StoryRoot } from '../../../models';

describe('StoryRootDisplay', () => {
    const mockStoryRoot: StoryRoot = {
        story_root_id: 'test-id',
        genre: 'Fantasy',
        tone: 'Epic',
        thematic_pillars: 'Courage, Honor, Sacrifice',
        notes: 'Optional notes here',
    };

    it('should render Story Root display with all fields', () => {
        render(<StoryRootDisplay storyRoot={mockStoryRoot} />);
        
        expect(screen.getByText('Story Root')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Fantasy')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Epic')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Courage, Honor, Sacrifice')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Optional notes here')).toBeInTheDocument();
    });

    it('should handle Story Root without notes', () => {
        const storyRootWithoutNotes: StoryRoot = {
            ...mockStoryRoot,
            notes: undefined,
        };
        
        render(<StoryRootDisplay storyRoot={storyRootWithoutNotes} />);
        
        expect(screen.getByText('Story Root')).toBeInTheDocument();
        expect(screen.queryByLabelText('Notes')).not.toBeInTheDocument();
    });

    it('should use custom title when provided', () => {
        render(<StoryRootDisplay storyRoot={mockStoryRoot} title="Custom Title" />);
        
        expect(screen.getByText('Custom Title')).toBeInTheDocument();
    });

    it('should render all fields as read-only', () => {
        render(<StoryRootDisplay storyRoot={mockStoryRoot} />);
        
        const genreField = screen.getByDisplayValue('Fantasy');
        expect(genreField).toHaveAttribute('readOnly');
    });
});
