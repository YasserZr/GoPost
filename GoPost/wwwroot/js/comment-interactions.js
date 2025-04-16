document.addEventListener('DOMContentLoaded', function() {
    // Like button functionality
    document.querySelectorAll('.reaction-btn').forEach(button => {
        button.addEventListener('click', function() {
            const commentId = this.dataset.commentId;
            
            // Toggle visual feedback immediately
            this.classList.toggle('reacted');
            
            // Heart animation effect
            const heart = document.createElement('span');
            heart.innerHTML = '❤️';
            heart.style.position = 'absolute';
            heart.style.left = (this.offsetLeft + this.offsetWidth / 2) + 'px';
            heart.style.top = (this.offsetTop + this.offsetHeight / 2) + 'px';
            heart.style.pointerEvents = 'none';
            heart.style.fontSize = '16px';
            heart.style.transform = 'translate(-50%, -50%) scale(0)';
            heart.style.opacity = '0';
            heart.style.transition = 'all 0.5s ease';
            
            this.parentNode.appendChild(heart);
            
            // Animate the heart
            setTimeout(() => {
                heart.style.transform = 'translate(-50%, -50%) scale(1.5)';
                heart.style.opacity = '1';
                
                setTimeout(() => {
                    heart.style.transform = 'translate(-50%, -120%) scale(1)';
                    heart.style.opacity = '0';
                    
                    setTimeout(() => {
                        heart.remove();
                    }, 500);
                }, 300);
            }, 10);
            
            // Send to server (implement this endpoint)
            fetch(`/Comments/ToggleLike/${commentId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            })
            .then(response => {
                if (!response.ok) {
                    this.classList.toggle('reacted'); // Revert visual state if server error
                }
                return response.json();
            })
            .then(data => {
                // Update like count if returned
                if (data && data.likeCount !== undefined) {
                    const likeText = this.textContent.replace(/[0-9]/g, '').trim();
                    this.textContent = `${likeText} ${data.likeCount}`;
                }
            })
            .catch(error => {
                console.error('Error toggling comment like:', error);
                this.classList.toggle('reacted'); // Revert visual state on error
            });
        });
    });

    // New comment form expansion
    const commentTextareas = document.querySelectorAll('.comment-form textarea');
    commentTextareas.forEach(textarea => {
        textarea.addEventListener('focus', function() {
            this.closest('.comment-form').classList.add('expanded');
        });
    });

    // New comment submission with animation
    const commentForms = document.querySelectorAll('.comment-form form');
    commentForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const textarea = this.querySelector('textarea');
            const content = textarea.value.trim();
            
            if (!content) {
                e.preventDefault();
                return;
            }
            
            // You could add a loading state here
            this.querySelector('button[type="submit"]').disabled = true;
            
            // If you want to handle this with AJAX instead of form submission:
            /* 
            e.preventDefault();
            
            const formData = new FormData(this);
            
            fetch(this.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Append new comment to the list with animation
                    const newComment = createCommentElement(data.comment);
                    document.querySelector('.comments-list').prepend(newComment);
                    textarea.value = '';
                    this.querySelector('button[type="submit"]').disabled = false;
                    
                    // Update comment count
                    const countElement = document.querySelector('.comments-title');
                    if (countElement) {
                        const currentCount = parseInt(countElement.textContent.match(/\d+/)[0] || '0');
                        countElement.textContent = countElement.textContent.replace(/\d+/, currentCount + 1);
                    }
                }
            });
            */
        });
    });

    // Function to create comment element (for AJAX responses)
    function createCommentElement(comment) {
        const commentElement = document.createElement('div');
        commentElement.className = 'comment-card';
        commentElement.id = `comment-${comment.id}`;
        commentElement.style.opacity = '0';
        commentElement.style.transform = 'translateY(10px)';
        
        // Build your comment HTML here based on the data structure
        
        // Animate entrance
        setTimeout(() => {
            commentElement.style.transition = 'all 0.3s ease';
            commentElement.style.opacity = '1';
            commentElement.style.transform = 'translateY(0)';
        }, 10);
        
        return commentElement;
    }
});
