# Getting Started with ASP.NET API

Use [MongoDB](https://www.mongodb.com) for database.

## Available api calls:

#### Login: `http://localhost:5000/api/auth/login`

#### Register: `http://localhost:5000/api/auth/register`

#### Get Auth User: `http://localhost:5000/api/auth/user`

#### Logout: `http://localhost:5000/api/auth/logout`

#### Get User By Username: `http://localhost:5000/api/users?username={username}`

#### Update User: `http://localhost:5000/api/users/{id}`

#### Follow User: `http://localhost:5000/api/users/d464f2d20d0f492db5d0276a/follow`

#### Unfollow User: `http://localhost:5000/api/users/d464f2d20d0f492db5d0276a/unfollow`

#### Get User By Id: `http://localhost:5000/api/users?id={id}`

#### Get All Users By Username: `http://localhost:5000/api/users/search?username={username}`

#### Get Post: `http://localhost:5000/api/posts/{id}`

#### Create Post: `http://localhost:5000/api/posts`

#### Update Post: `http://localhost:5000/api/posts/{id}`

#### Delete Post: `http://localhost:5000/api/posts/{id}`

#### Like/Dislike Post: `http://localhost:5000/api/posts/{id}/like`

#### Get Timeline: `http://localhost:5000/api/posts/timeline`

#### Get Posts By Username: `http://localhost:5000/api/posts/profile/{username}`

#### Add Comment: `http://localhost:5000/api/posts/{id}/comments`

#### Edit Comment: `http://localhost:5000/api/posts/{id}/comments/{commentId}`

#### Delete Comment: `http://localhost:5000/api/posts/{id}/comments/{commentId}`

#### Get User Friends: `http://localhost:5000/api/users/friends/{id}`

#### Upload User Image: `http://localhost:5000/api/users/upload`

##### Upload Post Image: `http://localhost:5000/api/post/upload`
